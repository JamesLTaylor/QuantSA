using System;
using ExcelDna.Integration;

namespace QuantSA.Excel.Common
{
    /// <summary>
    /// Marks a method that converts an object coming from excel to one of
    /// type <see cref="RequiredType"/>.  The method must have a signature that matches
    /// <see cref="ConverterDelegates.InputConverter0"/>.
    /// </summary>
    public class ExcelInputConverter0Attribute : Attribute
    {
        public Type RequiredType;
    }

    /// <summary>
    /// Marks a method that converts an object coming from QuantSA or a plug in to a type that
    /// Excel can handle.
    /// </summary>
    public class ExcelOutputConverter0Attribute : Attribute
    {
        public Type SuppliedType;
    }

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

    [AttributeUsage(AttributeTargets.Parameter)]
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