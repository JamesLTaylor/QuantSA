using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Office.Interop.Excel;

namespace PrepareRelease
{
    /// <summary>
    /// Check that all example sheets run without errors.
    /// </summary>
    internal class SpreadsheetChecker
    {
        /// <summary>
        /// The name of the file that stores all the error locations
        /// </summary>
        public const string ERROR_OUTPUT_FILE = "SpreadsheetChecker.csv";

        /// <summary>
        /// The name of the file that lists which functions are called by each spreadsheet.
        /// </summary>
        public const string SHEET_AND_FUNCS_FILE = "SheetsAndFuncs.csv";

        private readonly string exampleSheetPath;
        private List<string> sheetErrorInfo;

        private Dictionary<string, HashSet<string>> sheetsAndFuncs;
        private readonly string tempOutputPath;
        private readonly string xllPath;

        /// <summary>
        /// Set up the paths for finding files and writing results
        /// </summary>
        /// <param name="xllPath">The full path and name of the QuantSA xll file.</param>
        /// <param name="exampleSheetPath">The folder in which the example sheets are stored.</param>
        /// <param name="tempOutputPath">The folder where intermediate files should be written.</param>
        public SpreadsheetChecker(string xllPath, string exampleSheetPath, string tempOutputPath)
        {
            this.xllPath = xllPath;
            this.exampleSheetPath = exampleSheetPath;
            this.tempOutputPath = tempOutputPath;
        }

        /// <summary>
        /// Read the sheets and functions called in each sheet from <see cref="SHEET_AND_FUNCS_FILE"/>
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, HashSet<string>> GetSheetsAndFuncs()
        {
            var lines = File.ReadAllLines(Path.Combine(tempOutputPath, SHEET_AND_FUNCS_FILE));
            var localSheetsAndFuncs = new Dictionary<string, HashSet<string>>();
            foreach (var line in lines)
            {
                var sheet = line.Split(',')[0];
                var name = line.Split(',')[1];
                if (!localSheetsAndFuncs.ContainsKey(sheet)) localSheetsAndFuncs[sheet] = new HashSet<string>();
                localSheetsAndFuncs[sheet].Add(name);
            }

            return localSheetsAndFuncs;
        }

        /// <summary>
        /// Counts the number of failed sheets and write the failed cells to a file in 
        /// <see cref="tempOutputPath"/> whose name is contained in <see cref="ERROR_OUTPUT_FILE"/>
        /// </summary>
        /// <returns>the number of failed sheets.</returns>
        internal int Check()
        {
            sheetErrorInfo = new List<string>();
            sheetsAndFuncs = new Dictionary<string, HashSet<string>>();
            var failedSheets = 0;

            var excelApp = new Application();
            excelApp.RegisterXLL(xllPath);

            // Iterate over files in folder
            var directory = new DirectoryInfo(exampleSheetPath);
            var files = directory.GetFiles();
            //string[] fileEntries = Directory.GetFiles(exampleSheetPath);
            foreach (var fileName in files.Where(
                fileinfo => fileinfo.Extension.Equals(".xlsx") &&
                            !fileinfo.Attributes.HasFlag(FileAttributes.Hidden)).Select(fileinfo => fileinfo.FullName))
            {
                var wb = excelApp.Workbooks.Open(fileName);
                wb.ForceFullCalculation = true;
                excelApp.CalculateFull();
                if (!WorkbookIsOK(wb)) failedSheets++;
                wb.Close(false);
            }

            var sheetAndFuncsContent = new List<string>();
            foreach (var sheetname in sheetsAndFuncs.Keys)
            foreach (var funcName in sheetsAndFuncs[sheetname])
                sheetAndFuncsContent.Add(sheetname + "," + funcName);

            Directory.CreateDirectory(tempOutputPath);
            File.WriteAllLines(Path.Combine(tempOutputPath, SHEET_AND_FUNCS_FILE), sheetAndFuncsContent.ToArray());
            File.WriteAllLines(Path.Combine(tempOutputPath, ERROR_OUTPUT_FILE), sheetErrorInfo.ToArray());

            return failedSheets;
        }

        /// <summary>
        /// Check if a workbook runs without any errors.  #N/A errors are ignored.
        /// </summary>
        /// <param name="wb">the workbook to be checked.</param>
        /// <returns>True if the sheet runs OK</returns>
        private bool WorkbookIsOK(Workbook wb)
        {
            var errorCount = 0;
            Console.Write(wb.Name);
            var funcs = new HashSet<string>();
            foreach (Worksheet sheet in wb.Worksheets)
            {
                var usedRange = sheet.UsedRange;
                foreach (Range cell in usedRange.Cells)
                {
                    if (cell.HasFormula && cell.Formula.ToString().StartsWith("=QSA"))
                    {
                        string temp = cell.Formula.ToString();
                        temp = temp.Substring(1, temp.IndexOf('(') - 1);
                        funcs.Add(temp.Split('.')[1]);
                    }

                    if (cell.HasFormula && IsError(cell.Value))
                    {
                        errorCount++;
                        sheetErrorInfo.Add(wb.Name + "," + sheet.Name + "," + cell.Address + "," +
                                           cell.Text.ToString());
                    }
                }
            }

            sheetsAndFuncs.Add(wb.Name, funcs);
            if (errorCount > 0)
            {
                Console.WriteLine(" - " + errorCount + " errors.");
                return false;
            }

            Console.WriteLine(" - OK");
            return true;
        }

        /// <summary>
        /// Returns true for any excel error value except #N/A
        /// http://stackoverflow.com/a/2425170/5890940
        /// </summary>
        private bool IsXLCVErr(object obj)
        {
            if (obj is int)
            {
                var result = (int) obj;
                if (result != (int) CVErrEnum.ErrNA)
                    return true;
                return false;
            }

            return false;
        }


        private bool IsError(object obj)
        {
            if (IsXLCVErr(obj)) return true;
            var objValue = obj as string;
            if (objValue != null && objValue.StartsWith("ERROR")) return true;

            return false;
        }

        /// <summary>
        /// http://stackoverflow.com/a/2425170/5890940
        /// </summary>
        private enum CVErrEnum
        {
            ErrDiv0 = -2146826281,
            ErrNA = -2146826246,
            ErrName = -2146826259,
            ErrNull = -2146826288,
            ErrNum = -2146826252,
            ErrRef = -2146826265,
            ErrValue = -2146826273
        }
    }
}