using System;
using ExcelDna.Integration;
using QuantSA.Excel.Shared;
using QuantSA.General;
using QuantSA.General.Conventions.Compounding;
using QuantSA.Primitives.Dates;

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

        [QuantSAExcelFunction(Description = "Create a curve of dates and rates.",
            Name = "QSA2.CreateDatesAndRatesCurve",
            ExampleSheet = "GeneralSwap.xlsx",
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateDatesAndRatesCurve.html")]
        public static IDiscountingSource CreateDatesAndRatesCurve(
            [ExcelArgument(Description = "The dates at which the rates apply.")]
            Date[] dates,
            [ExcelArgument(Description = "The rates.")]
            double[] rates,
            [QuantSAExcelArgument(
                Description =
                    "Optional: The currency that this curve can be used for discounting.  Leave blank to use for any currency.",
                Default = "Currency.ANY")]
            Currency currency)
        {
            return new DatesAndRates(currency, dates[0], dates, rates);
        }

        [QuantSAExcelFunction(Description = "Get the discount factor from a curve object.  The DF will be from the anchor date until the supplied date.",
            Name = "QSA2.GetDF",
            Category = "QSA.Rates",
            ExampleSheet = "Introduction.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetDF.html")]
        public static double GetDF([ExcelArgument(Description = "The curve from which the DF is required.")]IDiscountingSource curve,
            [ExcelArgument(Description = "The date on which the discount factor is required.  Cannot be before the anchor date of the curve.")]Date date)
        {
            return curve.GetDF(date);
        }
    }
}