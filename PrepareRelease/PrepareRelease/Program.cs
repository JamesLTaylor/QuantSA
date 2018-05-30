using System;
using System.IO;

namespace PrepareRelease
{
    internal class Program
    {
        // Default values
        //TODO: Make it possible to overwrite these in args of main.
        private static readonly bool checkSpreadsheets = true;
        private static readonly bool generateHelp = true;
        private static readonly bool checkExampleSheetsAreValid = checkSpreadsheets && generateHelp && true;
        private static readonly bool generateInstallerResources = true;
        public static string Rootpath = @"c:\dev\QuantSA";
        public static string XllPath = @"C:\Dev\QuantSA\QuantSA\QuantSA.Excel.AddIn\bin\Debug\QuantSA.xll";

        private static void Main(string[] args)
        {
            // Variables
            var rootpath = Rootpath;
            var xllPath = Path.Combine(rootpath, XllPath);
            var exampleSheetPath = Path.Combine(rootpath, @"ExcelExamples");
            var tempOutputPath = Path.Combine(rootpath, @"temp");

            MarkdownGenerator generator = null;
            SpreadsheetChecker ssChecker = null;

            // Check spreadsheets
            if (checkSpreadsheets)
            {
                ssChecker = new SpreadsheetChecker(xllPath, exampleSheetPath, tempOutputPath);
                var failedSheets = ssChecker.Check();
                Console.WriteLine("Running example sheets: " + failedSheets + " errors. (see " +
                                  SpreadsheetChecker.ERROR_OUTPUT_FILE + ")");
            }

            // Generate the help
            if (generateHelp)
            {
                string[] dllsWithExposedFunctions =
                {
                    Path.Combine(rootpath, @"QuantSA\QuantSA.Excel.Addin\bin\Debug\QuantSA.Excel.Addin.dll"),
                    Path.Combine(rootpath, @"QuantSA\QuantSA.Excel.Addin\bin\Debug\QuantSA.ExcelFunctions.dll")
                };
                var outputPath = Path.Combine(rootpath, @"Documentation\");
                var helpURL = "http://www.quantsa.org/";
                generator = new MarkdownGenerator(dllsWithExposedFunctions, outputPath, helpURL, tempOutputPath);
                var failedMarkdown = generator.Generate();
                Console.WriteLine("Generating markdown: " + failedMarkdown + " errors. (see " +
                                  MarkdownGenerator.MD_ERROR_OUTPUT_FILE + ")");
            }

            // Check the example sheets are valid.  This means that each example sheet referenced in the 
            // help comments actually maps to an example sheet in the install directory.
            if (checkExampleSheetsAreValid)
            {
                if (!generateHelp || !checkSpreadsheets)
                    throw new Exception(
                        "Can only check if spreadsheets are valid if 'generateHelp' and 'checkSpreadsheets' are set to True.");
                var failedExampleSheetRefs = generator.AreAllExampleSheetsValid(ssChecker.GetSheetsAndFuncs());
                Console.WriteLine("Checking valid example sheets: " + failedExampleSheetRefs + " errors. (see " +
                                  MarkdownGenerator.SS_ERROR_OUTPUT_FILE + ")");
            }

            // Generate the installer resource files
            if (generateInstallerResources)
            {
                var installFilesPartialPath = Path.Combine(@"QuantSA\QuantSA.Excel.Addin\bin\Release");
                var installFileGenerator = new InstallerGenerator(rootpath, installFilesPartialPath);
                installFileGenerator.Generate();
            }


            // Wait for keyboard
            Console.WriteLine("Finished.  Press any key...");
            Console.ReadKey();
        }
    }
}