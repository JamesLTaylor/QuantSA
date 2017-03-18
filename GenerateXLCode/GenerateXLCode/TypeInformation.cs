using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateXLCode
{
    public class TypeInformation
    {
        /// <summary>
        /// Determines whether the output type of a function is primitive and will be written directly 
        /// to the cells or is not primitive and will be placed on the object map and a reference returned 
        /// to the cells.
        /// </summary>
        /// <param name="outputType">Type of the output.</param>
        /// <returns>
        ///   <c>true</c> if the output type is QuantSA primitive; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPrimitiveOutput(Type outputType)
        {
            Type type = outputType.IsArray ? outputType.GetElementType() : outputType;
            if (type == typeof(double)) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(int)) return true;
            return false;

        }


        /// <summary>
        /// Does the input type have an automatic conversion from Excel cell values.  If it does not
        /// then an object will need to be retrieved from the object map.  
        /// </summary>
        /// <remarks>
        /// If you update the values in this method then you probably need to update the values in
        /// PrepareRelease.MarkdownGenerator.InputTypeShouldHaveHelpLink 
        /// </remarks>
        /// <param name="inputType">Type of the input.</param>
        /// <returns></returns>
        public static bool ConvertibleFromString(Type inputType)
        {
            Type type = inputType.IsArray ? inputType.GetElementType() : inputType;
            if (type == typeof(string)) return true;
            if (type.Name == "Currency") return true;
            if (type.Name == "FloatingIndex") return true;
            if (type.Name == "Tenor") return true;
            if (type.Name == "Share") return true;
            if (type.Name == "ReferenceEntity") return true;
            if (type.Name == "CompoundingConvention") return true;
            if (type.Name == "DayCountConvention") return true;
            if (type.Name == "BusinessDayConvention") return true;
            if (type.Name == "Calendar") return true;            
            return false;
        }

        public static bool InputTypeHasCustomConversion(Type inputType)
        {
            Type type = inputType.IsArray ? inputType.GetElementType() : inputType;
            if (type == typeof(double)) return true;
            if (type == typeof(int)) return true;
            if (type == typeof(bool)) return true;
            if (type.Name == "Date") return true;
            return false;
        }
    }
}
