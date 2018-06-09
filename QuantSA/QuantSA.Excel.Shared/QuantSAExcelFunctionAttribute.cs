using System;
using ExcelDna.Integration;

namespace QuantSA.Excel.Shared
{
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class QuantSAExcelFunctionAttribute : ExcelFunctionAttribute
    {
        public string ExampleSheet = null;

        /// <summary>
        /// Does this excel function have a generated version?  If so construct the delegate that is registered in Excel from the 
        /// generated version but get all the help descriptions from this version.
        /// </summary>
        public bool HasGeneratedVersion = false;
    }
}