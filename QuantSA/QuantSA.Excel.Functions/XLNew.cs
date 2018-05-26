using System;
using QuantSA.Excel.Shared;
using QuantSA.General.Conventions.Compounding;

namespace QuantSA.ExcelFunctions
{
    public class XLNew
    {
        [QuantSAExcelFunction(Description = "Convert an interest rate from one compounding convention to another.",
            Name = "QSA2.RateConvert2",
            HasGeneratedVersion = true,
            Category = "QSA.Conventions",
            ExampleSheet = "Conventions.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/RateConvert.html")]
        public static double RateConvert2(
            double rate,
            [QuantSAExcelArgument(Description = "The compounding convention of the input rate.")]
            CompoundingConvention compoundingFrom,
            [QuantSAExcelArgument(Description = "The compounding convention that the output rate should be in.")]
            CompoundingConvention compoundingTo,
            [QuantSAExcelArgument(
                Description =
                    "The year fraction over which the rate applies.  Only required if one of the conventions is 'Simple' or 'Discount'",
                Default = "null")]
            double? yearFraction)
        {
            if (compoundingFrom == CompoundingStore.Simple && yearFraction == null)
                throw new ArgumentException(
                    "Cannot convert from a 'Simple' convention without the year fraction being specified.");
            if (compoundingFrom == CompoundingStore.Discount && yearFraction == null)
                throw new ArgumentException(
                    "Cannot convert from a 'Discount' convention without the year fraction being specified.");
            if (compoundingTo == CompoundingStore.Simple && yearFraction == null)
                throw new ArgumentException(
                    "Cannot convert from a 'Simple' convention without the year fraction being specified.");
            if (compoundingTo == CompoundingStore.Discount && yearFraction == null)
                throw new ArgumentException(
                    "Cannot convert to a 'Discount' convention without the year fraction being specified.");

            var dyf = yearFraction ?? 1.0;
            if (yearFraction < 1e-12)
                throw new ArgumentException("The provided year fraction must not be negative or zero.");
            var df = compoundingFrom.DF(rate, dyf);
            var resultRate = compoundingTo.rateFromDF(df, dyf);
            return resultRate;
        }
    }
}