using QuantSA.General;
using QuantSA.Excel.Common;
using QuantSA.General.Conventions.Compounding;
using System;

namespace QuantSA.ExcelFunctions
{
    public class XLConventions
    {
        [QuantSAExcelFunction(Description = "",
            Name = "QSA.RateConvert",
            HasGeneratedVersion = true,
            Category = "QSA.Conventions",
            ExampleSheet = "Conventions.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/RateConvert.html")]
        public static double RateConvert([QuantSAExcelArgument(Description = "The rate to convert.")]double rate,
            [QuantSAExcelArgument(Description = "")]CompoundingConvention compoundingFrom,
            [QuantSAExcelArgument(Description = "")]CompoundingConvention compoundingTo,
            [QuantSAExcelArgument(Description = "(Optional) The yearfraction over which the rate applies.  Only required if one of the conventions is 'Simple' or 'Discount'", Default = "double.NaN")]double yearFraction)

        {
            if (compoundingFrom == CompoundingStore.Simple && double.IsNaN(yearFraction))
                throw new ArgumentException("Cannot convert from a 'Simple' convention without the year fraction being specified.");
            if (compoundingFrom == CompoundingStore.Discount && double.IsNaN(yearFraction))
                throw new ArgumentException("Cannot convert from a 'Discount' convention without the year fraction being specified.");
            if (compoundingTo == CompoundingStore.Simple && double.IsNaN(yearFraction))
                throw new ArgumentException("Cannot convert from a 'Simple' convention without the year fraction being specified.");
            if (compoundingTo == CompoundingStore.Discount && double.IsNaN(yearFraction))
                throw new ArgumentException("Cannot convert to a 'Discount' convention without the year fraction being specified.");

            if (double.IsNaN(yearFraction)) yearFraction = 1.0;
            double df = compoundingFrom.DF(rate, yearFraction);
            double resultRate = compoundingTo.rateFromDF(df, yearFraction);
            return resultRate;
        }
    }
}