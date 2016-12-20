using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrepareRelease
{
    class Program
    {
        static void Main(string[] args)
        {
            // Variables
            string rootpath = @"c:\dev\QuantSA";
            string xllPath = Path.Combine(rootpath, @"QuantSA\Excel\bin\Debug\QuantSA.xll");
            string exampleSheetPath = Path.Combine(rootpath, @"ExcelExamples");
            string tempOutputPath = Path.Combine(rootpath, @"temp");

            // Check spreadsheets
            //SpreadsheetChecker ssChecker = new SpreadsheetChecker(xllPath, exampleSheetPath, tempOutputPath);
            //int failedSheets = ssChecker.Check();

            // Generate the help
            string[] dllsWithExposedFunctions = {
                Path.Combine(rootpath, @"QuantSA\Excel\bin\Debug\QuantSA.Excel.dll"),
                Path.Combine(rootpath, @"QuantSA\Excel\bin\Debug\QuantSA.ExcelFunctions.dll") };
            string outputPath = @"C:\Dev\jamesltaylor.github.io\";
            string helpURL = "http://www.quantsa.org/";
            MarkdownGenerator generator = new MarkdownGenerator(dllsWithExposedFunctions, outputPath, helpURL);
            generator.Generate();

        }
    }
}
