using System;
using System.Collections.Generic;
using System.Reflection;
using ExcelDna.Integration;
using QuantSA.Excel.Common;

namespace QuantSA.Excel.Addin.AddIn
{
    public static class ExcelFunctionRegistration
    {
        public static void Register(Assembly assembly)
        {
            var delegates = new List<Delegate>();
            var functionAttributes = new List<object>();
            var functionArgumentAttributes = new List<List<object>>();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsPublic) continue;
                var members = type.GetMembers();
                foreach (var member in members)
                {
                    var excelFuncAttr = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                    if (excelFuncAttr != null)
                    {
                        var method = member as MethodInfo;
                        var aAttr = new List<object>();
                        var defaults = new List<string>();
                        foreach (var param in method.GetParameters())
                        {
                            var argAttrib = param.GetCustomAttribute<QuantSAExcelArgumentAttribute>();
                            if (argAttrib == null)
                            {
                                aAttr.Add(new ExcelArgumentAttribute
                                {
                                    Name = param.Name,
                                    Description = param.ParameterType.Name
                                });
                                defaults.Add(null);
                            }
                            else
                            {
                                aAttr.Add(argAttrib);
                                defaults.Add(argAttrib.Default);
                            }
                        }

                        var dnaFuncAttr = excelFuncAttr.CreateExcelFunctionAttribute();
                        if (dnaFuncAttr.Name == null) dnaFuncAttr.Name = method.Name;
                        var excelFunction = new ExcelFunction(method, defaults);
                        delegates.Add(excelFunction.GetDelegate());
                        functionAttributes.Add(dnaFuncAttr);
                        functionArgumentAttributes.Add(aAttr);
                    }

                    var inputConverterAttr = member.GetCustomAttribute<ExcelInputConverter0Attribute>();
                    if (inputConverterAttr != null)
                        ExcelTypeConverter.AddInputConverter(inputConverterAttr.RequiredType, member as MethodInfo);
                    var outputConverterAttr = member.GetCustomAttribute<ExcelOutputConverter0Attribute>();
                    if (outputConverterAttr != null)
                        ExcelTypeConverter.AddOutputConverter(outputConverterAttr.SuppliedType, member as MethodInfo);
                }
            }

            ExcelIntegration.RegisterDelegates(delegates, functionAttributes, functionArgumentAttributes);
        }
    }
}