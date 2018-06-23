using System;
using System.Diagnostics;
using System.Text;
using ExcelDna.Integration;
using QuantSA.Excel.Shared;

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
    }
}