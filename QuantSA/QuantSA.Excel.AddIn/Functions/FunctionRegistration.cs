using System;
using System.Collections.Generic;
using System.Reflection;
using ExcelDna.Integration;
using QuantSA.Excel.Shared;

namespace QuantSA.Excel.Addin.Functions
{
    /// <summary>
    /// An object that can collect all the functions with the <see cref="ExcelFunctionAttribute"/>.
    /// </summary>
    public static class FunctionRegistration
    {
        public static List<string> FunctionNames = new List<string>();

        public static void RegisterFrom(Assembly assembly, string addInName)
        {
            var delegates = new List<Delegate>();
            var functionAttributes = new List<object>();
            var functionArgumentAttributes = new List<List<object>>();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsPublic) continue;
                foreach (var member in type.GetMembers())
                {
                    var excelFuncAttr = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                    if (excelFuncAttr == null) continue;
                    var parts = excelFuncAttr.Name.Split('.');
                    if (!(parts.Length == 2 && parts[0].Equals(addInName)))
                        throw new AddInException($"{excelFuncAttr.Name} does not following the naming " +
                                                 $"convention: {addInName}.FunctionName");
                    FunctionNames.Add(excelFuncAttr.Name);
                    var method = member as MethodInfo;
                    var aAttr = new List<object>();
                    var defaults = new List<string>();
                    if (method == null)
                        throw new AddInException($"{excelFuncAttr.Name} is marked with a " +
                                                 $"QuantSAExcelFunctionAttribute but is not a method");
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
            
            ExcelIntegration.RegisterDelegates(delegates, functionAttributes, functionArgumentAttributes);
        }
    }
}