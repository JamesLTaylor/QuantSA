using QuantSA.Core.Primitives;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.MarketData;

namespace QuantSA.CoreExtensions.ProductPVs.Rates
{
    public static class FloatLegASWPVs
    {
        /// <summary>
        /// Curve based valuation of <see cref="FloatLegASW"/>.  Assumes that correct forecast and discount curves have been provided.
        /// </summary>
        /// <param name="leg"></param>
        /// <param name="forecastCurve"></param>
        /// <param name="discountCurve"></param>
        /// <returns></returns>
        public static double CurvePV1(this FloatLegASW leg, IFloatingRateSource forecastCurve,
            IDiscountingSource discountCurve)
        {
            var legIndex = forecastCurve.GetFloatingIndex();
            var resetDates1 = leg.GetRequiredIndexDates(legIndex);
            var indexValues1 = new double[resetDates1.Count];
            for (var i = 0; i < resetDates1.Count; i++)
                indexValues1[i] = forecastCurve.GetForwardRate(resetDates1[i]);
            leg.SetIndexValues(legIndex, indexValues1);
            var cfs1 = leg.GetCFs();
            var value1 = cfs1.PV(discountCurve);
            return value1;
        }

        //This is for getting the denominator cashflows of the spread ASW calc
        public static double CurvePV2(this FloatLegASW leg, IFloatingRateSource forecastCurve,
        IDiscountingSource discountCurve)
        {
            var legIndex = forecastCurve.GetFloatingIndex();
            var resetDates1 = leg.GetRequiredIndexDates(legIndex);
            var indexValues1 = new double[resetDates1.Count];
            for (var i = 0; i < resetDates1.Count; i++)
                indexValues1[i] = 1;
            leg.SetIndexValues(legIndex, indexValues1);
            var cfs1 = leg.GetCFs();
            var value1 = cfs1.PV(discountCurve);
            return value1;
        }

    }
}
