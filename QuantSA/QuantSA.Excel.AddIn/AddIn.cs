using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using ExcelDna.Integration;
using QuantSA.Excel;
using QuantSA.Excel.Addin.AddIn;
using QuantSA.Excel.Common;
using QuantSA.Excel.Shared;
using QuantSA.ExcelFunctions;

/// <summary>
/// 
/// </summary>
/// <seealso cref="ExcelDna.Integration.IExcelAddIn" />
public class AddIn : IExcelAddIn
{
    public static bool Alreadyloaded = false;
    public static List<IQuantSAPlugin> plugins;
    public static Dictionary<string, Bitmap> assemblyImageResources;
    private List<Delegate> delegates;
    private List<List<object>> functionArgumentAttributes;
    private List<object> functionAttributes;

    private readonly string FunctionsFilenameAll =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
        "\\functions_all.csv"; // updated in build to include all functions and default visibility

    private readonly string FunctionsFilenameUser =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
        "\\functions_user.csv"; // user editable to control which function appear

    // Called when addin opens
    public void AutoOpen()
    {
        try
        {
            var pathString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "QuantSA");
            if (!Directory.Exists(pathString))
                Directory.CreateDirectory(pathString);
            //TODO: Check if newer version of addin exists.

            //Check if functions_all.csv is newer than functions_user.csv.  If it is then
            //merge the new functions in and use that to hide or show individual functions.
            UpdateUserFunctionFile();

            //Expose only those functions that appear in FunctionsFilenameUser with a true.  The
            //rest will still be there but as hidden.  So that users can share sheets.
            ExposeUserSelectedFunctions();

            //Check in the installation folder for any dlls that include a class of type IQuantSAPlugin
            plugins = new List<IQuantSAPlugin>();
            assemblyImageResources = new Dictionary<string, Bitmap>();
            ExposePlugins();
            foreach (var plugin in plugins)
            {
                plugin.SetObjectMap(ObjectMap.Instance);
                plugin.SetInstance(plugin);
            }
            var assemblies = new[]
            {
                Assembly.GetAssembly(typeof(XLEquities)),
                Assembly.GetAssembly(typeof(AddIn))
            };
            foreach (var assembly in assemblies)
                ExcelTypeConverter.AddConvertersFrom(assembly);
            foreach (var assembly in assemblies)
                FunctionRegistration.RegisterFrom(assembly);
            
        }
        catch (Exception e)
        {
            var pathString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "QuantSA");
            var fileName = Path.Combine(pathString, "QuantSAError.txt");
            File.WriteAllText(fileName, e.ToString());
            throw new Exception("An error occurred while opening the QuantSA addin.\n" +
                                "Check the error log file for details.\n\n" +
                                $"{fileName}");
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
        var funcsAndVisibility = GetFunctionVisibility(FunctionsFilenameUser);
        var functions = GetQuantSAFunctions();
        delegates = new List<Delegate>();
        functionAttributes = new List<object>();
        functionArgumentAttributes = new List<List<object>>();

        foreach (var entry in functions)
        {
            var method = (MethodInfo) entry.Value;
            var quantsaAttribute = method.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
            // If the function appears in the user function list use the isHidden value from there.  Otherwise use the default behaviour.
            bool? isHidden = null;
            if (funcsAndVisibility.ContainsKey(quantsaAttribute.Name))
                isHidden = !funcsAndVisibility[quantsaAttribute.Name];

            if (!quantsaAttribute.HasGeneratedVersion
            ) // Make delegates for all but the methods that have generated versions of themselves
            {
                //Create the delegate, Taken from: http://stackoverflow.com/a/16364220
                var thisDelegate = method.CreateDelegate(Expression.GetDelegateType(
                    (from parameter in method.GetParameters() select parameter.ParameterType)
                    .Concat(new[] {method.ReturnType})
                    .ToArray()));
                delegates.Add(thisDelegate);

                if (quantsaAttribute.IsGeneratedVersion)
                {
                    var manualMethod = (MethodInfo) functions["_" + entry.Key];
                    AddSingleAutoFunction(method, manualMethod, isHidden);
                }
                else
                {
                    AddSingleManualFunction(method, isHidden);
                }
            }
        }
    }


    /// <summary>
    /// Registers the single manual function.
    /// </summary>
    private void AddSingleManualFunction(MethodInfo method, bool? isHidden)
    {
        //Create the function attribute
        var quantsaAttribute = method.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
        if (isHidden != null) quantsaAttribute.IsHidden = isHidden.Value;
        functionAttributes.Add(quantsaAttribute);

        // Create the function argument attributes
        var thisArgumentAttributes = new List<object>();
        foreach (var param in method.GetParameters())
        {
            var argAttrib = param.GetCustomAttribute<ExcelArgumentAttribute>();
            if (argAttrib != null) argAttrib.Name = param.Name;
            thisArgumentAttributes.Add(argAttrib);
        }

        functionArgumentAttributes.Add(thisArgumentAttributes);
    }

    /// <summary>
    /// Registers a single function that is exposed by a manually and automatically written method.  
    /// see http://www.quantsa.org/home_expose_to_excel.html
    /// </summary>
    /// <param name="method">The generated method.</param>
    /// <param name="manualMethod">The manual method.</param>
    /// <param name="isHidden">Is this function hidden.</param>
    private void AddSingleAutoFunction(MethodInfo method, MethodInfo manualMethod, bool? isHidden)
    {
        //Create the function attribute
        var quantsaAttribute = manualMethod.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
        if (isHidden != null) quantsaAttribute.IsHidden = isHidden.Value;
        functionAttributes.Add(quantsaAttribute);

        // Create the function argument attributes
        var thisArgumentAttributes = new List<object>();
        if (manualMethod.GetParameters().Length < method.GetParameters().Length)
        {
            var argAttrib = new ExcelArgumentAttribute();
            argAttrib.Name = "objectName";
            argAttrib.Description = "The name of the object to be created.";
            //Note that the above 2 strings are the same as those added in GenerateDocs, if they are changed here they should be changed there too.
            thisArgumentAttributes.Add(argAttrib);
        }

        foreach (var param in manualMethod.GetParameters())
        {
            var argAttrib = param.GetCustomAttribute<ExcelArgumentAttribute>();
            if (argAttrib != null) argAttrib.Name = param.Name;
            if (ExcelUtilities.InputTypeShouldHaveHelpLink(param.ParameterType))
            {
                var typeName = param.ParameterType.IsArray
                    ? param.ParameterType.GetElementType().Name
                    : param.ParameterType.Name;
                argAttrib.Description += "(" + typeName + ")";
            }

            thisArgumentAttributes.Add(argAttrib);
        }

        functionArgumentAttributes.Add(thisArgumentAttributes);
    }


    /// <summary>
    /// If there is a new version of all functions then add the new functions into the user file.
    /// </summary>
    private void UpdateUserFunctionFile()
    {
        var funcsInUserFile = GetFunctionVisibility(FunctionsFilenameUser);
        var funcsInAllFile = GetFunctionVisibility(FunctionsFilenameAll);

        foreach (var entry in funcsInAllFile)
            if (!funcsInUserFile.ContainsKey(entry.Key))
                funcsInUserFile[entry.Key] = entry.Value;
        var list = funcsInUserFile.Keys.ToList();
        list.Sort();
        using (var file = new StreamWriter(FunctionsFilenameUser))
        {
            foreach (var key in list) file.WriteLine(key + "," + (funcsInUserFile[key] ? "yes" : "no"));
        }
    }


    /// <summary>
    /// Gets the list of available functions and checks if they should be exposed to this user.
    /// </summary>
    /// <returns></returns>
    private Dictionary<string, bool> GetFunctionVisibility(string functionFilename)
    {
        var funcsAndVisibility = new Dictionary<string, bool>();
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
            var line = reader.ReadLine();
            var cols = line.Split(',');
            bool visible;
            if (cols[1].Trim().ToLower().Equals("yes"))
                visible = true;
            else if (cols[1].Trim().ToLower().Equals("no"))
                visible = false;
            else
                throw new ArgumentException("file must contain only 'yes' or 'no' in the second column");

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
        var functions = GetQuantSAFunctions();
        var funtionVisibility = new Dictionary<string, bool>();
        foreach (var entry in functions)
            funtionVisibility[entry.Key] = !entry.Value.GetCustomAttribute<QuantSAExcelFunctionAttribute>().IsHidden;
        return funtionVisibility;
    }


    /// <summary>
    /// Use reflection on QuantSA.Excel.dll and QuantSA.ExcelFunctions.dll to find all the members that have 
    /// the <see cref="QuantSAExcelFunctionAttribute"/> attribute.
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, MemberInfo> GetQuantSAFunctions()
    {
        var quantSAFunctions = new Dictionary<string, MemberInfo>();

        var assembly1 = Assembly.GetAssembly(typeof(XLGeneral));
        UpdateFunctionsFromAssembly(assembly1, quantSAFunctions);
        var assembly2 = Assembly.GetAssembly(typeof(XLEquities));
        UpdateFunctionsFromAssembly(assembly2, quantSAFunctions);

        return quantSAFunctions;
    }

    /// <summary>
    /// Updates the functions from assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="quantSAFunctions">The quant sa functions.</param>
    private static void UpdateFunctionsFromAssembly(Assembly assembly, Dictionary<string, MemberInfo> quantSAFunctions)
    {
        Type[] types = null;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            //TODO: This is for troubleshooting when the assembly does not load.  Should be written to some more general log.
            var sb = new StringBuilder();
            foreach (var exSub in ex.LoaderExceptions)
            {
                sb.AppendLine(exSub.Message);
                var exFileNotFound = exSub as FileNotFoundException;
                if (exFileNotFound != null)
                    if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exFileNotFound.FusionLog);
                    }

                sb.AppendLine();
            }

            var errorMessage = sb.ToString();
            Console.WriteLine(errorMessage);
        }

        foreach (var type in types)
        {
            if (!type.IsPublic) continue;

            var members = type.GetMembers();

            foreach (var member in members)
            {
                var attribute = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                if (attribute != null)
                {
                    if (attribute.HasGeneratedVersion
                    ) // if there is generated version then that will be used to construct the delgate and this one will be used to get the help.
                        quantSAFunctions["_" + attribute.Name] = member;
                    else
                        quantSAFunctions[attribute.Name] = member;
                }
            }
        }
    }


    /// <summary>
    /// Expose all plugins in the Plugins folder
    /// </summary>
    private void ExposePlugins()
    {
        var dllDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\Plugins";
        if (!Directory.Exists(dllDirectory)) return; // No plugin folder, return without doing anything.
        var fileEntries = Directory.GetFiles(dllDirectory);
        foreach (var file in fileEntries)
            if (Path.GetExtension(file).ToLower().Equals(".dll"))
                try
                {
                    ExposePlugin(file);
                }
                catch (Exception e)
                {
                    var le = new ExcelMessage(e);
                    le.ShowDialog();
                }
    }

    /// <summary>
    /// Expose a plugin from a dll filename, if the dll is a valid plugin.
    /// </summary>
    private void ExposePlugin(string filename)
    {
        var DLL = Assembly.LoadFile(filename);
        var names = DLL.GetManifestResourceNames();
        //ResourceSet set = new ResourceSet(names[0]);

        string shortName = null;
        foreach (var type in DLL.GetExportedTypes())
            if (typeof(IQuantSAPlugin).IsAssignableFrom(type)) // This class is a QuantSA plugin
            {
                var plugin = Activator.CreateInstance(type) as IQuantSAPlugin;
                shortName = plugin.GetShortName();
                plugins.Add(plugin);
            }

        if (shortName == null)
            throw new Exception(Path.GetFileName(filename) + " is in the Plugins directory but is not a valid plugin.");

        foreach (var type in DLL.GetExportedTypes())
        foreach (var member in type.GetMembers())
        {
            var attribute = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
            if (attribute != null) // We have found an excel exposed function.
            {
                if (!attribute.Category.Equals(shortName))
                    throw new Exception(attribute.Name + " in plugin " + shortName + " is not in the excel category " +
                                        shortName);
                var parts = attribute.Name.Split('.');
                if (!(parts.Length == 2 && parts[0].Equals(shortName)))
                    throw new Exception(attribute.Name + " in plugin " + shortName +
                                        "does not following the naming convention: " + shortName + ".FunctionName");

                // TODO: Check that the category and naming are all acceptable
                UpdateDelgatesAndAtribs((MethodInfo) member);
            }

            if (member.MemberType.Equals(MemberTypes.Method))
                if (((MethodInfo) member).ReturnType.Equals(typeof(Bitmap)))
                {
                    var image = ((MethodInfo) member).Invoke(null, null) as Bitmap;
                    assemblyImageResources.Add(member.Name.Substring(4), image);
                }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    private void UpdateDelgatesAndAtribs(MethodInfo method)
    {
        //Create the delegate, Taken from: http://stackoverflow.com/a/16364220
        var thisDelegate = method.CreateDelegate(Expression.GetDelegateType(
            (from parameter in method.GetParameters() select parameter.ParameterType)
            .Concat(new[] {method.ReturnType})
            .ToArray()));
        delegates.Add(thisDelegate);

        //Create the function attribute
        var quantsaAttribute = method.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
        functionAttributes.Add(quantsaAttribute);

        // Create the function argument attributes
        var thisArgumentAttributes = new List<object>();
        foreach (var param in method.GetParameters())
        {
            var argAttrib = param.GetCustomAttribute<ExcelArgumentAttribute>();
            if (argAttrib != null) argAttrib.Name = param.Name;
            thisArgumentAttributes.Add(argAttrib);
        }

        functionArgumentAttributes.Add(thisArgumentAttributes);
    }
}