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
        // Default values
        //TODO: Make it possible to overwrite these in args of main.
        static bool checkSpreadsheets = true;
        static bool generateHelp = true;
        static bool checkExampleSheetsAreValid = checkSpreadsheets && generateHelp && true;
        static bool generateInstallerResources = true;

        static void Main(string[] args)
        {
            // Variables
            string rootpath = @"c:\dev\QuantSA"; 
            string xllPath = Path.Combine(rootpath, @"QuantSA\ExcelAddin\bin\Debug\QuantSA.xll");
            string exampleSheetPath = Path.Combine(rootpath, @"ExcelExamples");
            string tempOutputPath = Path.Combine(rootpath, @"temp");

            MarkdownGenerator generator = null;
            SpreadsheetChecker ssChecker = null;

            // Check spreadsheets
            if (checkSpreadsheets) {
                ssChecker = new SpreadsheetChecker(xllPath, exampleSheetPath, tempOutputPath);
                int failedSheets = ssChecker.Check();
                Console.WriteLine("Running example sheets: " + failedSheets.ToString() + " errors. (see " + SpreadsheetChecker.ERROR_OUTPUT_FILE + ")");
            }

            // Generate the help
            if (generateHelp) {
                string[] dllsWithExposedFunctions = {
                Path.Combine(rootpath, @"QuantSA\ExcelAddin\bin\Debug\QuantSA.Excel.Addin.dll"),
                Path.Combine(rootpath, @"QuantSA\ExcelAddin\bin\Debug\QuantSA.ExcelFunctions.dll") };
                string outputPath = Path.Combine(rootpath, @"Documentation\");
                string helpURL = "http://www.quantsa.org/";
                generator = new MarkdownGenerator(dllsWithExposedFunctions, outputPath, helpURL, tempOutputPath);
                int failedMarkdown = generator.Generate();
                Console.WriteLine("Generating markdown: " + failedMarkdown.ToString() + " errors. (see " + MarkdownGenerator.MD_ERROR_OUTPUT_FILE + ")");
            }

            // Check the example sheets are valid.  This means that each example sheet referenced in the 
            // help comments actually maps to an example sheet in the install directory.
            if (checkExampleSheetsAreValid)
            {
                if (!generateHelp || !checkSpreadsheets)
                    throw new Exception("Can only check if spreadsheets are valid if 'generateHelp' and 'checkSpreadsheets' are set to True.");
                int failedExampleSheetRefs = generator.AreAllExampleSheetsValid(ssChecker.GetSheetsAndFuncs());
                Console.WriteLine("Checking valid example sheets: " + failedExampleSheetRefs.ToString() + " errors. (see " + MarkdownGenerator.SS_ERROR_OUTPUT_FILE + ")");
            }

            // Generate the installer resource files
            if (generateInstallerResources)
            {
                string installFilesPartialPath = Path.Combine(@"QuantSA\ExcelAddin\bin\Release");
                InstallerGenerator installFileGenerator = new InstallerGenerator(rootpath, installFilesPartialPath);
                installFileGenerator.Generate();
            }


            // Wait for keyboard
            Console.ReadKey();

        }
    }
}
