using System;
using System.Collections.Generic;
using System.Reflection;
using ExcelDna.Integration;
using log4net;
using QuantSA.Excel.Shared;
using QuantSA.Shared.State;

namespace QuantSA.Excel.Addin.Functions
{
    /// <summary>
    /// An object that can collect all the functions with the <see cref="QuantSAExcelFunctionAttribute"/>.
    /// </summary>
    public static class FunctionRegistration
    {
        static FunctionRegistration()
        {
        }

        private static readonly ILog Log = QuantSAState.LogFactory.Get(MethodBase.GetCurrentMethod().DeclaringType);

        public static List<string> FunctionNames = new List<string>();

        /// <summary>
        /// Register all eligible functions in <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="addInName">This will be used to ensure that all functions start with the
        /// '[<paramref name="addInName"/>].' That means the QuantSA functions need to start with
        /// 'QSA' and plugin function need to start with <see cref="IQuantSAPlugin.GetShortName"/>.</param>
        /// <param name="funcsInUserFile"></param>
        public static void RegisterFrom(Assembly assembly, string addInName, Dictionary<string, bool> funcsInUserFile)
        {
            Log.Info($"Registering Excel functions from {assembly.FullName}");
            var delegates = new List<Delegate>();
            var functionAttributes = new List<object>();
            var functionArgumentAttributes = new List<List<object>>();
            GetDelegatesAndAttributes(assembly, addInName, funcsInUserFile, ref delegates, ref functionAttributes, ref functionArgumentAttributes);
            ExcelIntegration.RegisterDelegates(delegates, functionAttributes, functionArgumentAttributes);
        }

        /// <summary>
        /// Get all QuantSA delegates and attributes present in the assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="addInName"></param>
        /// <param name="delegates"></param>
        /// <param name="functionAttributes"></param>
        /// <param name="functionArgumentAttributes"></param>
        public static void GetDelegatesAndAttributes(Assembly assembly, string addInName,
            ref List<Delegate> delegates,
            ref List<object> functionAttributes, ref List<List<object>> functionArgumentAttributes)
        {
            GetDelegatesAndAttributes(assembly, addInName, new Dictionary<string, bool>(),
                ref delegates, ref functionAttributes, ref functionArgumentAttributes);
        }

        /// <summary>
        /// Get all the functions that appear in the assembly.  If the function name appears in
        /// <paramref name="funcsInUserFile"/> then use the value there to override the default
        /// visibility.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="addInName"></param>
        /// <param name="funcsInUserFile"></param>
        /// <param name="delegates"></param>
        /// <param name="functionAttributes"></param>
        /// <param name="functionArgumentAttributes"></param>
        public static void GetDelegatesAndAttributes(Assembly assembly, string addInName, 
            Dictionary<string, bool> funcsInUserFile, ref List<Delegate> delegates,
            ref List<object> functionAttributes, ref List<List<object>> functionArgumentAttributes)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsPublic) continue;
                foreach (var member in type.GetMembers())
                {
                    var excelFuncAttr = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                    var dnaExcelFuncAttr = member.GetCustomAttribute<ExcelFunctionAttribute>();
                    if (dnaExcelFuncAttr!=null && excelFuncAttr==null)
                        Log.Warn($"{dnaExcelFuncAttr.Name} is defined as an ExcelDNA function but not a QuantSA function so will be ignored");
                    if (excelFuncAttr == null) continue;
                    if (funcsInUserFile.ContainsKey(excelFuncAttr.Name))
                        excelFuncAttr.IsHidden = !funcsInUserFile[excelFuncAttr.Name];

                    var parts = excelFuncAttr.Name.Split('.');
                    if (!(parts.Length == 2 && parts[0].Equals(addInName)))
                        throw new AddInException($"{excelFuncAttr.Name} does not follow the naming " +
                                                 $"convention: {addInName}.FunctionName");
                    FunctionNames.Add(excelFuncAttr.Name);
                    var method = member as MethodInfo;
                    var aAttr = new List<object>();
                    var defaults = new List<string>();
                    if (method == null)
                        throw new AddInException($"{excelFuncAttr.Name} is marked with a " +
                                                 "QuantSAExcelFunctionAttribute but is not a method");
                    var putOnMap = ExcelTypeConverter.ShouldUseReference(method.ReturnType);
                    if (putOnMap)
                        aAttr.Add(new ExcelArgumentAttribute
                        {
                            Name = "objectName",
                            Description = "The name that this object will be assigned on the map. " +
                                          "Should be unique."
                        });
                    foreach (var param in method.GetParameters())
                    {
                        var argAttrib = param.GetCustomAttribute<QuantSAExcelArgumentAttribute>();
                        if (argAttrib == null)
                        {
                            var dnaAttrib = param.GetCustomAttribute<ExcelArgumentAttribute>();
                            if (dnaAttrib != null)
                            {
                                if (dnaAttrib.Name == null) dnaAttrib.Name = param.Name;
                                dnaAttrib.Description = "(" + param.ParameterType.Name + ")" + dnaAttrib.Description;
                                aAttr.Add(dnaAttrib);
                            }
                            else
                            {
                                aAttr.Add(new ExcelArgumentAttribute
                                {
                                    Name = param.Name,
                                    Description = param.ParameterType.Name
                                });
                            }

                            defaults.Add(string.Empty);
                        }

                        else
                        {
                            if (argAttrib.Name == null) argAttrib.Name = param.Name;
                            argAttrib.Description = "(" + param.ParameterType.Name + ")" + argAttrib.Description;
                            aAttr.Add(argAttrib);
                            if (argAttrib.Default != string.Empty)
                                argAttrib.Description = "*" + argAttrib.Description + $"(Default value = {argAttrib.Default})";
                            defaults.Add(argAttrib.Default);
                        }
                    }

                    var dnaFuncAttr = excelFuncAttr;
                    if (dnaFuncAttr.Name == null) dnaFuncAttr.Name = method.Name;
                    var excelFunction = new ExcelFunction(method, defaults, putOnMap);
                    delegates.Add(excelFunction.GetDelegate());
                    functionAttributes.Add(dnaFuncAttr);
                    functionArgumentAttributes.Add(aAttr);
                }
            }
        }
    }
}