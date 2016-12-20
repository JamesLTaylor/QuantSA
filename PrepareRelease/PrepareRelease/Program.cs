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
            SpreadsheetChecker ssChecker = new SpreadsheetChecker(xllPath, exampleSheetPath, tempOutputPath);
            int failedSheets = ssChecker.Check();
            

        }
    }
}
