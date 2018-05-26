using System;
using ExcelDna.Integration;

namespace QuantSA.Excel.Shared
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class QuantSAExcelArgumentAttribute : ExcelArgumentAttribute
    {
        /// <summary>
        /// The default value of this parameter.  It will be passed verbatim to the converter for this
        /// type so its form must match what the conversions method for this
        /// type of parameter expects.
        /// </summary>
        public string Default = string.Empty;
    }
}