using QuantSA.General.Products.SAMarket;
using System;
using QuantSA.General.Dates;
using QuantSA.General;

namespace QuantSA.ProductExtensions.SAMarket
{
    public static class BesaJseBondEx
    {
        public static class Keys
        {
            public const string roundedAip = "roundedAip";
            public const string roundedClean = "roundedClean";
            public const string unroundedAip = "unroundedAip";
            public const string unroundedClean = "unroundedClean";
            public const string unroundedAccrued = "unroundedAccrued";
            public const string tradingWithNextCoupon = "tradingWithNextCoupon";
        }

        private static Date GetLastCouponDateOnOrBefore(this BesaJseBond bond, Date settleDate)
        {
            Date thisYearCpn1 = new Date(settleDate.Year, bond.couponMonth1, bond.couponDay1);
            Date thisYearCpn2 = new Date(settleDate.Year, bond.couponMonth2, bond.couponDay2);
            Date lastYearCpn2 = new Date(settleDate.Year - 1, bond.couponMonth2, bond.couponDay2);

            if (settleDate > thisYearCpn2)
                return thisYearCpn2;
            else if (settleDate > thisYearCpn1)
                return thisYearCpn1;
            else
                return lastYearCpn2;
        }

        private static Date GetNextCouponDate(this BesaJseBond bond, Date couponDate)
        {
            if (couponDate.Month == bond.couponMonth2)
                return new Date(couponDate.Year + 1, bond.couponMonth1, bond.couponDay1);
            else
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
            double N = 100.0;
            double typicalCoupon = N * bond.annualCouponRate / 2;
            Date T0 = bond.GetLastCouponDateOnOrBefore(settleDate);
            Date T1 = bond.GetNextCouponDate(T0);
            int n = (int)Math.Round((bond.maturityDate - T1) / 182.625);
            bool tradingWithNextCoupon = (T1 - settleDate) > bond.booksCloseDateDays;
            int d = tradingWithNextCoupon ? settleDate - T0 : settleDate - T1;
            double unroundedAccrued = N * bond.annualCouponRate * d / 365.0;
            double roundedAccrued = Math.Round(unroundedAccrued, 5);
            double couponAtT1 = tradingWithNextCoupon ? typicalCoupon : 0.0;
            double V = 1 / (1 + ytm / 2);

            double brokenPeriodDf;
            if (n>0)            
                brokenPeriodDf = Math.Pow(V, ((double)T1 - settleDate) / (T1 - T0));
            else
                brokenPeriodDf = 1 / (1 + ytm * ((double)T1 - settleDate) / 365.0);

            double unroundedAip = brokenPeriodDf *
                (couponAtT1 + typicalCoupon * V * (1 - Math.Pow(V, n)) / (1 - V) + N * Math.Pow(V, n));

            double unroundedClean = unroundedAip - unroundedAccrued;
            double roundedClean = Math.Round(unroundedClean, 5);
            double roundedAip = roundedClean + roundedAccrued;
            
            ResultStore results = new ResultStore();
            results.Add(Keys.roundedAip, roundedAip);
            results.Add(Keys.roundedClean, roundedClean);
            results.Add(Keys.unroundedAip, unroundedAip);
            results.Add(Keys.unroundedClean, unroundedClean);
            results.Add(Keys.unroundedAccrued, unroundedAccrued);
            results.Add(Keys.tradingWithNextCoupon, tradingWithNextCoupon ? 1.0 : 0.0);
            
            return results; 
        }
    }

}
