using System;
using System.Collections.Generic;
using System.Reflection;
using ExcelDna.Integration;
using QuantSA.Excel.Common;

namespace QuantSA.Excel.Addin.AddIn
{
    /// <summary>
    /// An object that can collect all the functions with the <see cref="ExcelFunctionAttribute"/> and
    /// converters with the Excel input and output converter attributes: <see cref="ExcelInputConverter0Attribute"/>,
    /// <see cref="ExcelOutputConverter0Attribute"/>.
    /// </summary>
    public class Registration
    {
        private readonly List<Delegate> _delegates;
        private readonly List<object> _functionAttributes;
        private readonly List<List<object>> _functionArgumentAttributes;

        public Registration()
        {
            _delegates = new List<Delegate>();
            _functionAttributes = new List<object>();
            _functionArgumentAttributes = new List<List<object>>();
        }

        public void AddFunctionsAndConverters(Assembly assembly)
        {
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
                        if (method == null)
                            throw new ArgumentException(
                                $"{excelFuncAttr.Name} is marked with a QuantSAExcelFunctionAttribute but is not a method");
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
                        _delegates.Add(excelFunction.GetDelegate());
                        _functionAttributes.Add(dnaFuncAttr);
                        _functionArgumentAttributes.Add(aAttr);
                    }

                    var inputConverterAttr = member.GetCustomAttribute<ExcelInputConverter0Attribute>();
                    if (inputConverterAttr != null)
                        ExcelTypeConverter.AddInputConverter(inputConverterAttr.RequiredType, member as MethodInfo);
                    var outputConverterAttr = member.GetCustomAttribute<ExcelOutputConverter0Attribute>();
                    if (outputConverterAttr != null)
                        ExcelTypeConverter.AddOutputConverter(outputConverterAttr.SuppliedType, member as MethodInfo);
                }
            }
        }

        public void Register()
        {
            ExcelIntegration.RegisterDelegates(_delegates, _functionAttributes, _functionArgumentAttributes);
        }
    }
}