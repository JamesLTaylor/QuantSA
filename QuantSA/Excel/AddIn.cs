using QuantSA.Excel;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Resources;
using System.Drawing;

public class MyAddIn : IExcelAddIn
{
    string FunctionsFilenameAll = AppDomain.CurrentDomain.BaseDirectory + "\\functions_all.csv"; // updated in build to include all functions and default visibility
    string FunctionsFilenameUser = AppDomain.CurrentDomain.BaseDirectory + "\\functions_user.csv"; // user editable to control which function appear

    public static List<IQuantSAPlugin> plugins;
    public static Dictionary<string, Bitmap> assemblyImageResources;

    // Called when addin opens
    public void AutoOpen()
    {
        //TODO: Check if newer version of addin exists.

        //Check if functions_all.csv is newer than functions_user.csv.  If it is then
        //merge the new functions in and use that to hide or show inividual functions.
        UpdateUserFunctionFile();

        //Expose only those functions that appear in FunctionsFilenameUser with a true.  The
        //rest will still be there but as hidden.  So that users can share sheets.
        ExposeUserSelectedFunctions();

        //Check in the installation folder for any dlls that include a class of type IQuantSAPlugin
        plugins = new List<IQuantSAPlugin>();
        assemblyImageResources = new Dictionary<string, Bitmap>();
        ExposePlugins();
        foreach (IQuantSAPlugin plugin in plugins)
        {
            plugin.SetObjectMap(ObjectMap.Instance);
            plugin.SetInstance(plugin);
        }
    }

    public void AutoClose()
    {
    }
    
    /// <summary>
    /// Hides or shows Excel function based on the contents of functions_user.csv
    /// </summary>
    public void ExposeUserSelectedFunctions()
    {
        //TODO: Add a default option so that visibility is controlled by release sometimes.
        Dictionary<string, bool> funcsAndVisibility = GetFunctionVisibility(FunctionsFilenameUser);
        Dictionary<string, MemberInfo> functions = GetQuantSAFunctions();

        List<Delegate> delegates = new List<Delegate>();
        List<object> functionAttributes = new List<object>();
        List<List<object>> functionArgumentAttributes = new List<List<object>>();
        
        foreach (KeyValuePair<string, MemberInfo> entry in functions)
        {
            MethodInfo method = ((MethodInfo)entry.Value);
            //Create the delgate, Taken from: http://stackoverflow.com/a/16364220
            Delegate thisDelegate  = method.CreateDelegate(Expression.GetDelegateType(
                    (from parameter in method.GetParameters() select parameter.ParameterType)
                    .Concat(new[] { method.ReturnType })
                    .ToArray()));
            delegates.Add(thisDelegate);

            //Create the function attribute
            QuantSAExcelFunctionAttribute quantsaAttribute = entry.Value.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
            // If the function appears in the user function list use the isHidden value from there.  Otherwise use the default behaviour.
            if (funcsAndVisibility.ContainsKey(quantsaAttribute.Name))
            {
                quantsaAttribute.IsHidden = !funcsAndVisibility[quantsaAttribute.Name];
            }
            functionAttributes.Add(quantsaAttribute.CreateExcelFunctionAttribute());            

            // Create the function argument attributes
            List<object> thisArgumentAttributes = new List<object>();
            foreach (ParameterInfo param in method.GetParameters())
            {
                var argAttrib = param.GetCustomAttribute<ExcelArgumentAttribute>();
                if (argAttrib != null)
                { 
                    argAttrib.Name = param.Name;
                }
                thisArgumentAttributes.Add(argAttrib);
            }
            functionArgumentAttributes.Add(thisArgumentAttributes);

        }
        ExcelIntegration.RegisterDelegates(delegates, functionAttributes, functionArgumentAttributes);
    }


    /// <summary>
    /// If there is a new version of all functions then add the new functions into the user file.
    /// </summary>
    private void UpdateUserFunctionFile()
    {
        Dictionary<string, bool> funcsInUserFile = GetFunctionVisibility(FunctionsFilenameUser);
        Dictionary<string, bool> funcsInAllFile = GetFunctionVisibility(FunctionsFilenameAll);
        
        foreach (KeyValuePair<string, bool> entry in funcsInAllFile)
        {
            if (!funcsInUserFile.ContainsKey(entry.Key))
            {
                funcsInUserFile[entry.Key] = entry.Value;
            }
        }
        List<string> list = funcsInUserFile.Keys.ToList();
        list.Sort();
        using (StreamWriter file = new StreamWriter(FunctionsFilenameUser))
        {
            foreach (string key in list)
            {
                file.WriteLine(key + "," + (funcsInUserFile[key] ? "yes" : "no"));
            }
        }
    }


    /// <summary>
    /// Gets the list of available functions and checks if they should be exposed to this user.
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, bool> GetFunctionVisibility(string functionFilename)
    {
        Dictionary<string, bool> funcsAndVisibility = new Dictionary<string, bool>();
        StreamReader reader;
        try
        {
            reader = new StreamReader(File.OpenRead(functionFilename));
        }
        catch (FileNotFoundException)
        {
            return funcsAndVisibility;
        }
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] cols = line.Split(',');
            bool visible;
            if (cols[1].Trim().ToLower().Equals("yes"))
            { visible = true; }
            else if (cols[1].Trim().ToLower().Equals("no"))
            { visible = false; }
            else
            { throw new ArgumentException("file must contain only 'yes' or 'no' in the second column"); }

            funcsAndVisibility[cols[0].Trim()] = visible;
        }
        reader.Close();
        return funcsAndVisibility;
    }

    /// <summary>
    /// Use reflection on Excel.dll to find all the members that have 
    /// the <see cref="QuantSAExcelFunctionAttribute"/> attribute.
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, bool> GetQuantSAFunctionVisibility()
    {
        Dictionary<string, MemberInfo> functions = GetQuantSAFunctions();
        Dictionary<string, bool> funtionVisibility = new Dictionary<string, bool>();
        foreach (KeyValuePair<string, MemberInfo> entry in functions)
        {
            funtionVisibility[entry.Key] = !entry.Value.GetCustomAttribute<QuantSAExcelFunctionAttribute>().IsHidden;
        }
        return funtionVisibility;
    }


    /// <summary>
    /// Use reflection on Excel.dll to find all the members that have 
    /// the <see cref="QuantSAExcelFunctionAttribute"/> attribute.
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, MemberInfo> GetQuantSAFunctions()
    {
        Dictionary<string, MemberInfo> quantSAFunctions = new Dictionary<string, MemberInfo>();
        Assembly excelAssemby = Assembly.GetAssembly(typeof(XLGeneral));
        Type[] types = null;
        try
        {
            types = excelAssemby.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            //TODO: This is for troubleshooting when the assembly does not load.  Should be written to some more general log.
            StringBuilder sb = new StringBuilder();
            foreach (Exception exSub in ex.LoaderExceptions)
            {
                sb.AppendLine(exSub.Message);
                FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                if (exFileNotFound != null)
                {
                    if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exFileNotFound.FusionLog);
                    }
                }
                sb.AppendLine();
            }
            string errorMessage = sb.ToString();
            Console.WriteLine(errorMessage);
        }
        foreach (Type type in types)
        {
            if (!type.IsPublic)
            {
                continue;
            }

            MemberInfo[] members = type.GetMembers();

            foreach (MemberInfo member in members)
            {
                QuantSAExcelFunctionAttribute attribute = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                if (attribute != null)
                {
                    quantSAFunctions[attribute.Name] = member;
                }
            }
        }
        return quantSAFunctions;
    }


    /// <summary>
    /// Expose all plugins in the Plugins folder
    /// </summary>
    private void ExposePlugins()
    {
        string dllDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\Plugins";
        if (!Directory.Exists(dllDirectory)) return; // No plugin folder, return without doing anything.
        string[] fileEntries = Directory.GetFiles(dllDirectory);
        foreach (string file in fileEntries)
        {
            if (Path.GetExtension(file).ToLower().Equals(".dll"))
            {
                try {
                    ExposePlugin(file);
                }
                catch (Exception e)
                {
                    ExcelMessage le = new ExcelMessage(e);
                    le.ShowDialog();
                }
            }
        }
    }

    /// <summary>
    /// Expose a plugin from a dll filename, if the dll is a valid plugin.
    /// </summary>
    private void ExposePlugin(string filename)
    {
        Assembly DLL = Assembly.LoadFile(filename);
        string[] names = DLL.GetManifestResourceNames();
        //ResourceSet set = new ResourceSet(names[0]);

        string shortName = null;
        foreach (Type type in DLL.GetExportedTypes())
        {
            if (typeof(IQuantSAPlugin).IsAssignableFrom(type)) // This class is a QuantSA plugin
            {
                IQuantSAPlugin plugin = Activator.CreateInstance(type) as IQuantSAPlugin;
                shortName = plugin.GetShortName();
                MyAddIn.plugins.Add(plugin);
            }
        }
        if (shortName == null)
        {
            throw new Exception(Path.GetFileName(filename) + " is in the Plugins directory but is not a valid plugin.");
        }

        List<Delegate> delegates = new List<Delegate>();
        List<object> functionAttributes = new List<object>();
        List<List<object>> functionArgumentAttributes = new List<List<object>>();
        foreach (Type type in DLL.GetExportedTypes())
        {
            foreach (MemberInfo member in type.GetMembers())
            {
                QuantSAExcelFunctionAttribute attribute = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                if (attribute != null) // We have found an excel exposed function.
                {
                    if (!attribute.Category.Equals(shortName)) throw new Exception(attribute.Name + " in plugin " + shortName + " is not in the excel category " + shortName);
                    string[] parts = attribute.Name.Split('.');
                    if (!(parts.Length == 2 && parts[0].Equals(shortName))) throw new Exception(attribute.Name + " in plugin " + shortName + "does not following the naming convention: " + shortName + ".FunctionName");  

                    // TODO: Check that the category and naming are all acceptable
                    UpdateDelgatesAndAtribs((MethodInfo)member, delegates, functionAttributes, functionArgumentAttributes);
                }
                if (member.MemberType.Equals(MemberTypes.Method))
                    if (((MethodInfo)member).ReturnType.Equals(typeof(System.Drawing.Bitmap)))
                    {
                        Bitmap image = ((MethodInfo)member).Invoke(null, null) as Bitmap;
                        assemblyImageResources.Add(member.Name.Substring(4), image);                        
                    }
            }
            
        }
        ExcelIntegration.RegisterDelegates(delegates, functionAttributes, functionArgumentAttributes);
    }
       
  

    /// <summary>
    /// 
    /// </summary>
    private void UpdateDelgatesAndAtribs(MethodInfo method, List<Delegate> delegates, List<object> functionAttributes, List<List<object>> functionArgumentAttributes)
    {
        //Create the delgate, Taken from: http://stackoverflow.com/a/16364220
        Delegate thisDelegate = method.CreateDelegate(Expression.GetDelegateType(
                (from parameter in method.GetParameters() select parameter.ParameterType)
                .Concat(new[] { method.ReturnType })
                .ToArray()));
        delegates.Add(thisDelegate);

        //Create the function attribute
        QuantSAExcelFunctionAttribute quantsaAttribute = method.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
        functionAttributes.Add(quantsaAttribute.CreateExcelFunctionAttribute());

        // Create the function argument attributes
        List<object> thisArgumentAttributes = new List<object>();
        foreach (ParameterInfo param in method.GetParameters())
        {
            var argAttrib = param.GetCustomAttribute<ExcelArgumentAttribute>();
            if (argAttrib != null)
            {
                argAttrib.Name = param.Name;
            }
            thisArgumentAttributes.Add(argAttrib);
        }
        functionArgumentAttributes.Add(thisArgumentAttributes);
    }

}