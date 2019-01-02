using System;
using QuantSA.Excel.Shared;

namespace QuantSA.Excel.Addin.TypeConverters
{
    public class EnumConverter : IInputConverter
    {
        public Type RequiredType => typeof(Enum);

        public object Convert(object input, string inputName, string defaultValue, Type requiredType)
        {
            var strValue = input == null ? defaultValue : input as string;
            if (strValue is null)
                throw new ArgumentException($"{inputName} must be a string representing a {requiredType.Name}.");
            try
            {
                var enumInstance = Enum.Parse(requiredType, strValue, true);
                return enumInstance;
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"{inputName} must be a string representing a {requiredType.Name}.");
            }
        }
    }
}