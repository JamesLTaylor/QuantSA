using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.Excel
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class QuantSAExcelFunctionAttribute : Attribute
    {
        public string Name = null;
        public string Description = null;
        public string Category = null;
        public string HelpTopic = null;
        public string ExampleSheet = null;
        public bool IsVolatile = false;
        /// <summary>
        /// Default behavior.  Can be overwritten by functions.csv file.
        /// </summary>
        public bool IsHidden = true;  
        public bool IsExceptionSafe = false;
        public bool IsMacroType = false;
        public bool IsThreadSafe = false;
        public bool IsClusterSafe = false;
        public bool ExplicitRegistration = false;
        public bool SuppressOverwriteError = false;

        public ExcelFunctionAttribute CreateExcelFunctionAttribute()
        {
            return new ExcelFunctionAttribute
            {
                Name = Name,
                Description = Description,
                Category = Category,
                HelpTopic = HelpTopic,
                IsVolatile = IsVolatile,
                IsHidden = IsHidden,
                IsExceptionSafe = IsExceptionSafe,
                IsMacroType = IsMacroType,
                IsThreadSafe = IsThreadSafe,
                IsClusterSafe = IsClusterSafe,
                ExplicitRegistration = ExplicitRegistration
            };
        }
    }
}
