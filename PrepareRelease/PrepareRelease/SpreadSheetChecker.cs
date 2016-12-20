using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace PrepareRelease
{
    /// <summary>
    /// Check that all example sheets run without errors.
    /// </summary>
    class SpreadsheetChecker
    {
        /// <summary>
        /// The name of the file that stores all the error locations
        /// </summary>
        public const string ERROR_OUTPUT_FILE = "SpreadsheetChecker.csv";
        /// <summary>
        /// The name of the file that lists which functions are called by each spreadsheet.
        /// </summary>
        public const string SHEET_AND_FUNCS_FILE = "SheetsAndFuncs.csv";

        Dictionary<string, HashSet<string>> sheetsAndFuncs;
        List<string> sheetErrorInfo;       
        private string xllPath;
        private string exampleSheetPath;
        private string tempOutputPath;

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
        /// Counts the number of failed sheets and write the failed cells to a file in 
        /// <see cref="tempOutputPath"/> whose name is contained in <see cref="ERROR_OUTPUT_FILE"/>
        /// </summary>
        /// <returns>the number of failed sheets.</returns>
        internal int Check()
        {
            sheetErrorInfo = new List<string>();
            sheetsAndFuncs = new Dictionary<string, HashSet<string>>();
            int failedSheets = 0;

            Excel.Application excelApp = new Excel.Application();
            excelApp.RegisterXLL(xllPath);

            // Iterate over files in folder
            DirectoryInfo directory = new DirectoryInfo(exampleSheetPath);
            FileInfo[] files = directory.GetFiles();
            //string[] fileEntries = Directory.GetFiles(exampleSheetPath);
            foreach (string fileName in files.Where(
                fileinfo => fileinfo.Extension.Equals(".xlsx") &&
                !fileinfo.Attributes.HasFlag(FileAttributes.Hidden)).Select(fileinfo => fileinfo.FullName))
            {          
                Excel.Workbook wb = excelApp.Workbooks.Open(fileName);                
                wb.ForceFullCalculation = true;
                excelApp.CalculateFull();
                if (!WorkbookIsOK(wb)) failedSheets++;  
                wb.Close(false);
            }

            List<string> sheetAndFuncsContent = new List<string>();
            foreach (string sheetname in sheetsAndFuncs.Keys)
            {
                foreach (string funcName in sheetsAndFuncs[sheetname]) {
                    sheetAndFuncsContent.Add(sheetname + "," + funcName);
                }
            }
            File.WriteAllLines(Path.Combine(tempOutputPath, SHEET_AND_FUNCS_FILE), sheetAndFuncsContent.ToArray());
            File.WriteAllLines(Path.Combine(tempOutputPath, ERROR_OUTPUT_FILE), sheetErrorInfo.ToArray());

            return failedSheets;
        }

        /// <summary>
        /// Check if a workbook runs without any errors.  #N/A erros are ignored.
        /// </summary>
        /// <param name="wb">the workbook to be checked.</param>
        /// <returns>True if the sheet runs OK</returns>
        private bool WorkbookIsOK(Excel.Workbook wb)
        {
            int errorCount = 0;
            Console.Write(wb.Name);
            HashSet<string> funcs = new HashSet<string>();
            foreach (Excel.Worksheet sheet in wb.Worksheets)
            {
                Excel.Range usedRange = sheet.UsedRange;
                foreach (Excel.Range cell in usedRange.Cells)
                {
                    if (cell.HasFormula && cell.Formula.ToString().StartsWith("=QSA"))
                    {
                        string temp = cell.Formula.ToString();
                        temp = temp.Substring(1, temp.IndexOf('(')-1);
                        funcs.Add(temp);
                    }

                    if (cell.HasFormula && IsError(cell.Value))
                    {
                        errorCount++;
                        sheetErrorInfo.Add(wb.Name + "," + sheet.Name +"," + cell.Address +"," + cell.Text.ToString());
                        //Console.WriteLine(wb.Name + "," + sheet.Name + "," + cell.Address + "," + cell.Text.ToString());                        
                    }
                }
            }
            sheetsAndFuncs.Add(wb.Name, funcs);
            if (errorCount > 0)
            {
                Console.WriteLine(" - " + errorCount.ToString() + " errors.");
                return false;
            }
            else
            {
                Console.WriteLine(" - OK");
                return true;
            }
        }

        /// <summary>
        /// http://stackoverflow.com/a/2425170/5890940
        /// </summary>
        enum CVErrEnum : Int32
        {
            ErrDiv0 = -2146826281,
            ErrNA = -2146826246,
            ErrName = -2146826259,
            ErrNull = -2146826288,
            ErrNum = -2146826252,
            ErrRef = -2146826265,
            ErrValue = -2146826273
        }

        /// <summary>
        /// Returns true for any excel error value except #N/A
        /// http://stackoverflow.com/a/2425170/5890940
        /// </summary>
        bool IsXLCVErr(object obj)
        {
            if ((obj) is Int32)
            {
                Int32 result = (Int32)obj;
                if (result != (Int32)CVErrEnum.ErrNA)
                    return true;
                else
                    return false;                    
            }
            else
                return false;
        }


        bool IsError(object obj)
        {
            if (IsXLCVErr(obj)) return true;
            string objValue = obj as string;
            if (objValue != null && objValue.StartsWith("ERROR")) return true;

            return false;
        }

    }
}

