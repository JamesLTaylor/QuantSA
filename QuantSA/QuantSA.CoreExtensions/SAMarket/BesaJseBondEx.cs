using System;
using QuantSA.General;
using QuantSA.General.Products.SAMarket;
using QuantSA.Primitives.Dates;

namespace QuantSA.ProductExtensions.SAMarket
{
    public static class BesaJseBondEx
    {
        private static Date GetLastCouponDateOnOrBefore(this BesaJseBond bond, Date settleDate)
        {
            var thisYearCpn1 = new Date(settleDate.Year, bond.couponMonth1, bond.couponDay1);
            var thisYearCpn2 = new Date(settleDate.Year, bond.couponMonth2, bond.couponDay2);
            var lastYearCpn2 = new Date(settleDate.Year - 1, bond.couponMonth2, bond.couponDay2);

            if (settleDate > thisYearCpn2)
                return thisYearCpn2;
            if (settleDate > thisYearCpn1)
                return thisYearCpn1;
            return lastYearCpn2;
        }

        private static Date GetNextCouponDate(this BesaJseBond bond, Date couponDate)
        {
            if (couponDate.Month == bond.couponMonth2)
                return new Date(couponDate.Year + 1, bond.couponMonth1, bond.couponDay1);
            return new Date(couponDate.Year, bond.couponMonth2, bond.couponDay2);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns></returns>
        public static ResultStore GetSpotMeasures(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var N = 100.0;
            var typicalCoupon = N * bond.annualCouponRate / 2;
            var T0 = bond.GetLastCouponDateOnOrBefore(settleDate);
            var T1 = bond.GetNextCouponDate(T0);
            var n = (int) Math.Round((bond.maturityDate - T1) / 182.625);
            var tradingWithNextCoupon = T1 - settleDate > bond.booksCloseDateDays;
            var d = tradingWithNextCoupon ? settleDate - T0 : settleDate - T1;
            var unroundedAccrued = N * bond.annualCouponRate * d / 365.0;
            var roundedAccrued = Math.Round(unroundedAccrued, 5);
            var couponAtT1 = tradingWithNextCoupon ? typicalCoupon : 0.0;
            var V = 1 / (1 + ytm / 2);

            double brokenPeriodDf;
            if (n > 0)
                brokenPeriodDf = Math.Pow(V, ((double) T1 - settleDate) / (T1 - T0));
            else
                brokenPeriodDf = 1 / (1 + ytm * ((double) T1 - settleDate) / 365.0);

            var unroundedAip = brokenPeriodDf *
                               (couponAtT1 + typicalCoupon * V * (1 - Math.Pow(V, n)) / (1 - V) + N * Math.Pow(V, n));

            var unroundedClean = unroundedAip - unroundedAccrued;
            var roundedClean = Math.Round(unroundedClean, 5);
            var roundedAip = roundedClean + roundedAccrued;

            var results = new ResultStore();
            results.Add(Keys.roundedAip, roundedAip);
            results.Add(Keys.roundedClean, roundedClean);
            results.Add(Keys.unroundedAip, unroundedAip);
            results.Add(Keys.unroundedClean, unroundedClean);
            results.Add(Keys.unroundedAccrued, unroundedAccrued);
            results.Add(Keys.tradingWithNextCoupon, tradingWithNextCoupon ? 1.0 : 0.0);

            return results;
        }

        public static class Keys
        {
            public const string roundedAip = "roundedAip";
            public const string roundedClean = "roundedClean";
            public const string unroundedAip = "unroundedAip";
            public const string unroundedClean = "unroundedClean";
            public const string unroundedAccrued = "unroundedAccrued";
            public const string tradingWithNextCoupon = "tradingWithNextCoupon";
        }
    }
}