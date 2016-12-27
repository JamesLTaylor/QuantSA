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
            string xllPath = Path.Combine(rootpath, @"QuantSA\ExcelAddin\bin\Debug\QuantSA.xll");
            string exampleSheetPath = Path.Combine(rootpath, @"ExcelExamples");
            string tempOutputPath = Path.Combine(rootpath, @"temp");

            /*
            // Check spreadsheets
            SpreadsheetChecker ssChecker = new SpreadsheetChecker(xllPath, exampleSheetPath, tempOutputPath);
            int failedSheets = ssChecker.Check();
            Console.WriteLine("Running example sheets: " + failedSheets.ToString() + " errors. (see " + SpreadsheetChecker.ERROR_OUTPUT_FILE + ")");

            // Generate the help
            string[] dllsWithExposedFunctions = {
                Path.Combine(rootpath, @"QuantSA\Excel\bin\Debug\QuantSA.Excel.dll"),
                Path.Combine(rootpath, @"QuantSA\Excel\bin\Debug\QuantSA.ExcelFunctions.dll") };
            string outputPath = @"C:\Dev\jamesltaylor.github.io\";
            string helpURL = "http://www.quantsa.org/";
            MarkdownGenerator generator = new MarkdownGenerator(dllsWithExposedFunctions, outputPath, helpURL, tempOutputPath);
            int failedMarkdown = generator.Generate();
            Console.WriteLine("Generating markdown: " + failedMarkdown.ToString() + " errors. (see " + MarkdownGenerator.MD_ERROR_OUTPUT_FILE + ")");

            // Check the example sheets are valid
            int failedExampleSheetRefs = generator.AreAllExampleSheetsValid(ssChecker.GetSheetsAndFuncs());
            Console.WriteLine("Checking valid example sheets: " + failedExampleSheetRefs.ToString() + " errors. (see " + MarkdownGenerator.SS_ERROR_OUTPUT_FILE + ")");
            */
            // Generate the installer resource files
            string installFilesPartialPath = Path.Combine(@"QuantSA\ExcelAddin\bin\Release");
            InstallerGenerator installFileGenerator = new InstallerGenerator(rootpath, installFilesPartialPath);
            installFileGenerator.Generate();


            // Wait for keyboard
            Console.ReadKey();

        }
    }
}
