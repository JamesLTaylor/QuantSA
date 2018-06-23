using System;
using QuantSA.Excel.Shared;
using QuantSA.Shared.Conventions.Compounding;

namespace QuantSA.Excel.Addin.TypeConverters
{
    public class CompoundingConverter : IInputConverter
    {
        public Type RequiredType => typeof(ICompoundingConvention);

        public object Convert(object input, string inputName, string defaultValue)
        {
            var strValue = input != null ? input as string : defaultValue;
            if (strValue == null)
                throw new ArgumentException(
                    $"{inputName}: Input must be a string representing a compounding convention.");
            switch (strValue.ToUpper())
            {
                case "SIMPLE": return CompoundingStore.Simple;
                case "DISCOUNT": return CompoundingStore.Discount;
                case "C":
                case "NACC":
                case "CONTINUOUS": return CompoundingStore.Continuous;
                case "D":
                case "NACD":
                case "DAILY": return CompoundingStore.Daily;
                case "M":
                case "NACM":
                case "MONTHLY": return CompoundingStore.Monthly;
                case "Q":
                case "NACQ":
                case "QUARTERLY": return CompoundingStore.Quarterly;
                case "S":
                case "NACS":
                case "SEMIANNUAL": return CompoundingStore.SemiAnnual;
                case "A":
                case "NACA":
                case "ANNUAL": return CompoundingStore.Annual;
                default:
                    throw new ArgumentException(strValue + " is not a known compounding convention in input: " +
                                                inputName);
            }
        }
    }
}