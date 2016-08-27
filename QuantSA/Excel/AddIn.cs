using Excel;
using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

public class MyAddIn : IExcelAddIn
{
    string FunctionsFilenameAll = AppDomain.CurrentDomain.BaseDirectory + "\\functions_all.csv"; // updated in build to include all functions and default visibility
    string FunctionsFilenameUser = AppDomain.CurrentDomain.BaseDirectory + "\\functions_user.csv"; // user editable to control which function appear

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
    }

    public void AutoClose()
    {
    }
    
    /// <summary>
    /// Hides or shows Excel function based on the contents of functions_user.csv
    /// </summary>
    public void ExposeUserSelectedFunctions()
    {
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
            quantsaAttribute.IsHidden = !funcsAndVisibility[quantsaAttribute.Name];
            functionAttributes.Add(quantsaAttribute.CreateExcelFunctionAttribute());            

            // Create the function argument attributes
            List<object> thisArgumentAttributes = new List<object>();
            foreach (ParameterInfo param in method.GetParameters())
            {
                var argAttrib = param.GetCustomAttribute<ExcelArgumentAttribute>();
                argAttrib.Name = param.Name;
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
        Assembly excelAssemby = Assembly.GetAssembly(typeof(BasicFunctions));
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

}