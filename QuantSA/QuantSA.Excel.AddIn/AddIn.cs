using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public static List<Tuple<IQuantSAPlugin, Assembly>> Plugins = new List<Tuple<IQuantSAPlugin, Assembly>>();
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
            if (!Directory.Exists(pathString)) Directory.CreateDirectory(pathString);
            //TODO: Check if newer version of addin exists.

            //Check if functions_all.csv is newer than functions_user.csv.  If it is then
            //merge the new functions in and use that to hide or show individual functions.
            UpdateUserFunctionFile();

            //Expose only those functions that appear in FunctionsFilenameUser with a true.  The
            //rest will still be there but as hidden.  So that users can share sheets.
            ExposeUserSelectedFunctions();

            //Check in the installation folder for any dlls that include a class of type IQuantSAPlugin
            AssemblyImageResources = new Dictionary<string, Bitmap>();
            GetPlugins();
            var assemblies = new[]
            {
                Assembly.GetAssembly(typeof(XLEquities)),
                Assembly.GetAssembly(typeof(AddIn))
            };
            foreach (var tuple in Plugins)
                ExcelTypeConverter.AddConvertersFrom(tuple.Item2);
            foreach (var assembly in assemblies)
                ExcelTypeConverter.AddConvertersFrom(assembly);
            foreach (var tuple in Plugins)
                FunctionRegistration.RegisterFrom(tuple.Item2, tuple.Item1.GetShortName());
            foreach (var assembly in assemblies)
                FunctionRegistration.RegisterFrom(assembly, "QSA");
        }
        catch (Exception e)
        {
            var pathString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "QuantSA");
            var fileName = Path.Combine(pathString, "QuantSAError.txt");
            File.WriteAllText(fileName, e.ToString());
            throw new AddInException("An error occurred while opening the QuantSA addin.\n" +
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
                throw new AddInException("file must contain only 'yes' or 'no' in the second column");

            funcsAndVisibility[cols[0].Trim()] = visible;
        }

        reader.Close();
        return funcsAndVisibility;
    }

    /// <summary>
    /// Expose all plugins in the Plugins folder
    /// </summary>
    private void GetPlugins()
    {
        var dllDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\Plugins";
        if (!Directory.Exists(dllDirectory)) return; // No plugin folder, return without doing anything.
        var fileEntries = Directory.GetFiles(dllDirectory);
        foreach (var file in fileEntries)
            if (Path.GetExtension(file).ToLower().Equals(".dll"))
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    var plugin = GetPlugin(assembly);
                    Plugins.Add(Tuple.Create(plugin, assembly));
                }
                catch (Exception e)
                {
                    var le = new ExcelMessage(e);
                    le.ShowDialog();
                }
    }

    /// <summary>
    /// Get an <see cref="IQuantSAPlugin"/> from an assembly or throw an <see cref="AddInException"/>
    /// </summary>
    private static IQuantSAPlugin GetPlugin(Assembly assembly)
    {
        foreach (var type in assembly.GetExportedTypes())
            if (typeof(IQuantSAPlugin).IsAssignableFrom(type)) // This class is a QuantSA plugin
            {
                return Activator.CreateInstance(type) as IQuantSAPlugin;
            }

        throw new AddInException($"{Path.GetFileName(assembly.FullName)} is in the Plugins " +
                                 "directory but is not a valid plugin.");
    }
}