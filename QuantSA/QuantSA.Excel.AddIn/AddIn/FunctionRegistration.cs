using System;
using System.Collections.Generic;
using System.Reflection;
using ExcelDna.Integration;
using QuantSA.Excel.Shared;

namespace QuantSA.Excel.Addin.AddIn
{
    /// <summary>
    /// An object that can collect all the functions with the <see cref="ExcelFunctionAttribute"/> and
    /// converters that implement <see cref="IInputConverter"/> or <see cref="IOutputConverter"/>.
    /// </summary>
    public static class FunctionRegistration
    {
        public static void RegisterFrom(Assembly assembly)
        {
            var delegates = new List<Delegate>();
            var functionAttributes = new List<object>();
            var functionArgumentAttributes = new List<List<object>>();
            UpdateDelegatesAndAttributes(assembly, ref delegates, ref functionAttributes, ref functionArgumentAttributes);
            ExcelIntegration.RegisterDelegates(delegates, functionAttributes, functionArgumentAttributes);
        }

        public static void UpdateDelegatesAndAttributes(Assembly assembly, ref List<Delegate> delegates, 
            ref List<object> functionAttributes, ref List<List<object>> functionArgumentAttributes)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsPublic) continue;
                foreach (var member in type.GetMembers())
                {
                    var excelFuncAttr = member.GetCustomAttribute<QuantSAExcelFunctionAttribute>();
                    if (excelFuncAttr != null)
                    {
                        var method = member as MethodInfo;
                        var aAttr = new List<object>();
                        var defaults = new List<string>();
                        if (method == null)
                            throw new ArgumentException(
                                $"{excelFuncAttr.Name} is marked with a QuantSAExcelFunctionAttribute but is not a method");
                        var putOnMap = ExcelTypeConverter.ShouldUseReference(method.ReturnType);
                        if (putOnMap)
                            aAttr.Add(new ExcelArgumentAttribute
                            {
                                Name = "objectName",
                                Description = "The name that this object will be assigned on the map.  Should be unique."
                            });
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

                        var dnaFuncAttr = excelFuncAttr.CreateExcelFunctionAttribute();
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
}