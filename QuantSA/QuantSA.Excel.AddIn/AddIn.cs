using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using ExcelDna.Integration;
using log4net;
using QuantSA.Excel;
using QuantSA.Excel.Addin.Config;
using QuantSA.Excel.Addin.Functions;
using QuantSA.Excel.Shared;
using QuantSA.ExcelFunctions;
using QuantSA.Shared.State;
using StaticData = QuantSA.Excel.Addin.Config.StaticData;

/// <summary>
/// 
/// </summary>
/// <seealso cref="ExcelDna.Integration.IExcelAddIn" />
public class AddIn : IExcelAddIn
{
    /// <summary>
    /// List of Plugin instance and assembly from which it was loaded.
    /// </summary>
    public static List<Tuple<IQuantSAPlugin, Assembly>> Plugins = new List<Tuple<IQuantSAPlugin, Assembly>>();

    public static Dictionary<string, Bitmap> AssemblyImageResources;

    private static readonly string FunctionsFilenameUser =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
        "\\QuantSA\\functions_user.csv"; // user editable to control which function appear

    private ILog _log;

    // Called when addin opens
    public void AutoOpen()
    {
        try
        {
            QuantSAState.SetLogger(new ExcelFileLogFactory());
            ExcelDna.IntelliSense.IntelliSenseServer.Install();
            _log = QuantSAState.LogFactory.Get(MethodBase.GetCurrentMethod().DeclaringType);
            
            var pathString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "QuantSA");
            if (!Directory.Exists(pathString)) Directory.CreateDirectory(pathString);
            //TODO: Check if newer version of addin exists.

            _log.Info("Loading static data");
            StaticData.Load();

            _log.Info("Check custom function visibility");
            // Get the functions that are set for the user.
            var funcsInUserFile = GetFunctionVisibility(FunctionsFilenameUser);
            
            //Check in the installation folder for any dlls that include a class of type IQuantSAPlugin
            AssemblyImageResources = new Dictionary<string, Bitmap>();
            _log.Info("Check for plugins");
            GetPlugins();
            var assemblies = new[]
            {
                Assembly.GetAssembly(typeof(XLEquities)),
                Assembly.GetAssembly(typeof(AddIn))
            };
            _log.Info("Add converters");
            foreach (var tuple in Plugins)
                ExcelTypeConverter.AddConvertersFrom(tuple.Item2);
            foreach (var assembly in assemblies)
                ExcelTypeConverter.AddConvertersFrom(assembly);

            _log.Info("Register user functions");
            foreach (var tuple in Plugins)
                FunctionRegistration.RegisterFrom(tuple.Item2, tuple.Item1.GetShortName(), funcsInUserFile);
            foreach (var assembly in assemblies)
                FunctionRegistration.RegisterFrom(assembly, "QSA", funcsInUserFile);
            UpdateUserFunctionFile(funcsInUserFile, FunctionRegistration.FunctionNames);
        }
        catch (Exception e)
        {
            _log.Error(e);
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
        ExcelDna.IntelliSense.IntelliSenseServer.Uninstall();
    }

    /// <summary>
    /// If there is a new version of all functions then add the new functions into the user file.
    /// </summary>
    private void UpdateUserFunctionFile(Dictionary<string, bool> funcsInUserFile, List<string> functionNames)
    {
        foreach (var name in functionNames)
            if (!funcsInUserFile.ContainsKey(name))
                funcsInUserFile[name] = true;
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
    private static Dictionary<string, bool> GetFunctionVisibility(string functionFilename)
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