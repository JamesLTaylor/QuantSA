using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Excel.Addin.Config;
using QuantSA.Excel.Addin.Functions;
using QuantSA.ExcelFunctions;

namespace QuantSA.Excel.Addin.Tests.TypeConverters
{
    [TestClass]
    public class ConverterTests
    {
        public ExcelFunction LoadAndGetExcelFunction(string functionName)
        {
            var delegates = new List<Delegate>();
            var functionAttributes = new List<object>();
            var functionArgumentAttributes = new List<List<object>>();
            var assembly = Assembly.GetAssembly(typeof(XLEquities));
            StaticData.Load();
            ExcelTypeConverter.AddConvertersFrom(Assembly.GetAssembly(typeof(AddIn)));
            FunctionRegistration.GetDelegatesAndAttributes(assembly, "QSA", new Dictionary<string, bool>(),
                ref delegates, ref functionAttributes, ref functionArgumentAttributes);

            foreach (var d in delegates)
            {
                var func = (ExcelFunction)d.Target;
                if (func.GetName() == functionName)
                    return func;
            }

            throw new InvalidOperationException($"{functionName} is not an available Excel function.");
        }

        /// <summary>
        /// Check that the currency is correctly converted and the curve can be constructed.
        /// </summary>
        [TestMethod]
        public void TestThatCcyCanConvert()
        {
            var func = LoadAndGetExcelFunction("CreateDatesAndRatesCurve");
            var result = func.Eval(new object[,] { { "curvename" } }, new object[,] { { 1.0 } }, new object[,] { { 1.0 } },
                new object[,] { { "ZAR" } });
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Check that an enum is correctly converted and the black formula can be used.
        /// </summary>
        [TestMethod]
        public void TestThatEnumCanConvert()
        {
            var func = LoadAndGetExcelFunction("FormulaBlack");
            var result = func.Eval(new object[,] { { "call" } }, new object[,] { { 1.0 } },
                new object[,] { { 1.0 } }, new object[,] { { 1.0 } }, new object[,] { { 1.0 } },
                new object[,] { { 1.0 } });
            Assert.AreEqual(0.3829, (double)result[0, 0], 1e-4);
        }
    }
}