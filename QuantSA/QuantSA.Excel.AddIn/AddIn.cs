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
using QuantSA.Excel.Addin.Functions;
using QuantSA.Excel.Shared;
using QuantSA.ExcelFunctions;

/// <summary>
/// 
/// </summary>
/// <seealso cref="ExcelDna.Integration.IExcelAddIn" />
public class AddIn : IExcelAddIn
{
    public static List<IQuantSAPlugin> Plugins;
    public static Dictionary<string, Bitmap> AssemblyImageResources;

    private static readonly string FunctionsFilenameAll =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
        "\\functions_all.csv"; // updated in build to include all functions and default visibility

    private static readonly string FunctionsFilenameUser =
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
            Plugins = new List<IQuantSAPlugin>();
            AssemblyImageResources = new Dictionary<string, Bitmap>();
            ExposePlugins();
            foreach (var plugin in Plugins)
            {
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


    /// <summary>
    /// Hides or shows Excel function based on the contents of functions_user.csv
    /// </summary>
    public void ExposeUserSelectedFunctions()
    {
        var funcsAndVisibility = GetFunctionVisibility(FunctionsFilenameUser);
    }

    public void AutoClose()
    {
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
        string shortName = null;
        foreach (var type in DLL.GetExportedTypes())
            if (typeof(IQuantSAPlugin).IsAssignableFrom(type)) // This class is a QuantSA plugin
            {
                var plugin = Activator.CreateInstance(type) as IQuantSAPlugin;
                shortName = plugin.GetShortName();
                Plugins.Add(plugin);
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
            }

            if (member.MemberType.Equals(MemberTypes.Method))
                if (((MethodInfo) member).ReturnType.Equals(typeof(Bitmap)))
                {
                    var image = ((MethodInfo) member).Invoke(null, null) as Bitmap;
                    AssemblyImageResources.Add(member.Name.Substring(4), image);
                }
        }
    }
}