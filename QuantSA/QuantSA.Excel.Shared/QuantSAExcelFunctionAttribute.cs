using System;
using ExcelDna.Integration;

namespace QuantSA.Excel.Shared
{
    /// <summary>
    /// This Attribute is a replica of <see cref="ExcelFunctionAttribute"/> but the functions in 
    /// QuantSA have this attribute so that the ExcelDNA does not automatically expose them.  The
    /// data here is used to create an <see cref="ExcelFunctionAttribute"/> but with the <see cref="IsHidden"/>
    /// field controlled at runtime.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class QuantSAExcelFunctionAttribute : Attribute
    {
        public string Category = null;
        public string Description = null;
        public string ExampleSheet = null;
        public bool ExplicitRegistration = false;

        /// <summary>
        /// Does this excel function have a generated version?  If so construct the delegate that is registered in Excel from the 
        /// generated version but get all the help descriptions from this version.
        /// </summary>
        public bool HasGeneratedVersion = false;

        public string HelpTopic = null;
        public bool IsClusterSafe = false;
        public bool IsExceptionSafe = false;

        /// <summary>
        /// Is this excel function the generated version of another one?
        /// </summary>
        public bool IsGeneratedVersion = false;

        /// <summary>
        /// Default behavior.  Can be overwritten by functions.csv file.
        /// </summary>
        public bool IsHidden = true;

        public bool IsMacroType = false;
        public bool IsThreadSafe = false;
        public bool IsVolatile = false;
        public string Name = null;
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