using System;
using System.Diagnostics;
using System.Text;
using ExcelDna.Integration;
using QuantSA.Excel.Shared;
using QuantSA.General;

namespace QuantSA.Excel.Addin.Functions
{
    /// <summary>
    /// Non financial utility functions.
    /// </summary>
    public class ExposedUtils
    {
        [ExcelFunction(Description = "",
            Name = "QSA.LatestError",
            Category = "QSA.General",
            IsMacroType = true,
            IsHidden = true)]
        public static string LatestError()
        {
            if (ObjectMap.Instance.LatestException == null)
            {
                var latestError = new ExcelMessage("No errors have occurred.");
                latestError.ShowDialog();
            }
            else
            {
                var latestError = new ExcelMessage(ObjectMap.Instance.LatestException);
                latestError.ShowDialog();
            }

            return "";
        }

        [ExcelFunction(Description = "",
            Name = "QSA.OpenExampleSheetsDir",
            Category = "QSA.General",
            IsMacroType = true,
            IsHidden = true)]
        public static object OpenExampleSheetsDir()
        {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"\ExcelExamples");
            return "";
        }

        [QuantSAExcelFunction(Description = "Create a C# representation of data in a spreadsheet.",
            Name = "QSA.GetCSArray",
            Category = "QSA.General",
            ExampleSheet = "Introduction.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetCSArray.html")]
        public static string[] GetCSArray([ExcelArgument(Description = "The block of values you want to use in C#.")]
            object[,] data,
            [ExcelArgument(Description = "The number of decimal places each value must have in the string.")]
            double decimalPlaces)
        {
            var iDecimalPlaces = (int) decimalPlaces;
            var result = new string[data.GetLength(0)];
            StringBuilder sb;
            for (var i = 0; i < data.GetLength(0); i++)
            {
                sb = new StringBuilder();
                sb.Append(i == 0 ? "{{" : "{");
                for (var j = 0; j < data.GetLength(1); j++)
                {
                    if (j > 0) sb.Append(",");
                    var value = (double) data[i, j];
                    sb.Append(value.ToString($"F{iDecimalPlaces}"));
                }

                sb.Append(i == data.GetLength(0) - 1 ? "}}" : "},");
                result[i] = sb.ToString();
            }

            return result;
        }

        [QuantSAExcelFunction(Description = "Get a string representing the path in which QuantSA is " +
                                            "installed.",
            Name = "QSA.GetInstallPath",
            Category = "QSA.General",
            ExampleSheet = "CreateProductFromFile.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetInstallPath.html")]
        public static string GetInstallPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        [QuantSAExcelFunction(Description = "Get a list of available results in the results object.",
            Name = "QSA.GetAvailableResults",
            Category = "QSA.General",
            ExampleSheet = "ZARSwap.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetAvailableResults.html")]
        public static object[,] GetAvailableResults(
            [ExcelArgument(Description = "The results object as returned by call to another QuantSA function")]
            IProvidesResultStore resultStore)
        {
            var temp = resultStore.GetResultStore().GetNames();
            var column = new object[temp.Length, 1];
            for (var i = 0; i < temp.Length; i++) column[i, 0] = temp[i];
            return column;
        }

        [QuantSAExcelFunction(Description = "Get the stored results of a calculation from a results object.",
            Name = "QSA.GetResults",
            Category = "QSA.General",
            ExampleSheet = "ZARSwap.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetResults.html")]
        public static object[,] GetResults([ExcelArgument(Description =
                "The results object as returned by a call to another QuantSA function")]
            IProvidesResultStore resultStore,
            [ExcelArgument(Description =
                "The name of the result required.  Use QSA.GetAvailableResults to get a list of all available results in this object.")]
            string resultName)
        {
            if (resultStore.GetResultStore().IsDate(resultName))
                return resultStore.GetResultStore().GetDates(resultName);
            if (resultStore.GetResultStore().IsString(resultName))
                return resultStore.GetResultStore().GetStrings(resultName);
            return resultStore.GetResultStore().Get(resultName);
        }
    }
}