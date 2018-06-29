using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Solution.Test
{
    public static class TestHelpers
    {
        public static readonly Date AnchorDate = new Date("2018-06-23");
        public static readonly Currency ZAR = new Currency("ZAR");

        public static IRSwap ZARSwap()
        {
            return IRSwap.CreateZARSwap(0.07, true, 100, AnchorDate, Tenor.FromYears(2));
        }

        public static DatesAndRates FlatDiscountCurve()
        {
            var dates = new[] {AnchorDate, AnchorDate.AddMonths(120)};
            var rates = new[] {0.07, 0.07};
            return new DatesAndRates(ZAR, AnchorDate, dates, rates);
        }
    }
}