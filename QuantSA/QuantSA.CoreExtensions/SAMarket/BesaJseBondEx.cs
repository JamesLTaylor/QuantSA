using System;
using QuantSA.Core.Products.SAMarket;
using QuantSA.Shared;
using QuantSA.Shared.Dates;

namespace QuantSA.CoreExtensions.SAMarket
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
            var t0 = bond.GetLastCouponDateOnOrBefore(settleDate);
            var t1 = bond.GetNextCouponDate(t0);
            var n = (int) Math.Round((bond.maturityDate - t1) / 182.625);
            var tradingWithNextCoupon = t1 - settleDate > bond.booksCloseDateDays;
            var d = tradingWithNextCoupon ? settleDate - t0 : settleDate - t1;
            var unroundedAccrued = N * bond.annualCouponRate * d / 365.0;
            var roundedAccrued = Math.Round(unroundedAccrued, 5);
            var couponAtT1 = tradingWithNextCoupon ? typicalCoupon : 0.0;
            var v = 1 / (1 + ytm / 2);

            double brokenPeriodDf;
            if (n > 0)
                brokenPeriodDf = Math.Pow(v, ((double) t1 - settleDate) / (t1 - t0));
            else
                brokenPeriodDf = 1 / (1 + ytm * ((double) t1 - settleDate) / 365.0);

            var unroundedAip = brokenPeriodDf *
                               (couponAtT1 + typicalCoupon * v * (1 - Math.Pow(v, n)) / (1 - v) + N * Math.Pow(v, n));

            var unroundedClean = unroundedAip - unroundedAccrued;
            var roundedClean = Math.Round(unroundedClean, 5);
            var roundedAip = roundedClean + roundedAccrued;

            var results = new ResultStore();
            results.Add(Keys.RoundedAip, roundedAip);
            results.Add(Keys.RoundedClean, roundedClean);
            results.Add(Keys.UnroundedAip, unroundedAip);
            results.Add(Keys.UnroundedClean, unroundedClean);
            results.Add(Keys.UnroundedAccrued, unroundedAccrued);
            results.Add(Keys.TradingWithNextCoupon, tradingWithNextCoupon ? 1.0 : 0.0);

            return results;
        }

        public static class Keys
        {
            public const string RoundedAip = "roundedAip";
            public const string RoundedClean = "roundedClean";
            public const string UnroundedAip = "unroundedAip";
            public const string UnroundedClean = "unroundedClean";
            public const string UnroundedAccrued = "unroundedAccrued";
            public const string TradingWithNextCoupon = "tradingWithNextCoupon";
        }
    }
}