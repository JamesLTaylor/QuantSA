using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.Excel
{
    /// <summary>
    /// This Attribute is a replica of <see cref="ExcelFunctionAttribute"/> but the functions in 
    /// QuantSA have this attribute so that the ExcelDNA does not automatically expose them.  The
    /// data here is used to create an <see cref="ExcelFunctionAttribute"/> but with the <see cref="IsHidden"/>
    /// field controlled at runtime.
    /// </summary>
    /// <seealso cref="System.Attribute" />
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
        /// <summary>
        /// Does this excel function have a generated version?  If so construct the delegate that is registered in Excel from the 
        /// generated version but get all the help descriptions from this version.
        /// </summary>
        public bool HasGeneratedVersion = false;
        /// <summary>
        /// Is this excel function the generated version of another one?
        /// </summary>
        public bool IsGeneratedVersion = false;

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

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class QuantSAExcelArgumentAttribute : ExcelArgumentAttribute
    {
        /// <summary>
        /// The default value of this parameter.  It will be passed verbatim to the Excelutilities 
        /// conversion method and so its form must depend on what the conversions method for this
        /// type of parameter expects.
        /// </summary>
        public string Default = null;
    }

}
