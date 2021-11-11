using System;
using System.Collections.Generic;
using System.Linq;
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
        private static List<ResultStore> GetBondMeasures(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var BondMeasures = new List<ResultStore>();

            // Compute spot measures
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

            double brokenPeriod, brokenPeriodDf, dB_dZ, d2B_dZ2;
            if (n > 0)
            {
                brokenPeriod = ((double)t1 - settleDate) / (t1 - t0);
                brokenPeriodDf = Math.Pow(v, brokenPeriod);
                dB_dZ = brokenPeriod * Math.Pow(v, brokenPeriod - 1);
                d2B_dZ2 = brokenPeriod * (brokenPeriod - 1) * Math.Pow(v, brokenPeriod - 2);
            }
            else
            {
                brokenPeriod = ((double)t1 - settleDate) / 182.25;
                brokenPeriodDf = 1 / (1 + ytm * brokenPeriod / 2);
                dB_dZ = brokenPeriod / Math.Pow(v + brokenPeriod * (1 - v), 2);
                d2B_dZ2 = -2 * brokenPeriod * (1 - brokenPeriod) / Math.Pow(v + brokenPeriod * (1 - v), 3);
            }
                
            var A1 = (couponAtT1 + typicalCoupon * v * (1 - Math.Pow(v, n)) / (1 - v) + N * Math.Pow(v, n));
            var unroundedAip = brokenPeriodDf * A1;
                               

            var unroundedClean = unroundedAip - unroundedAccrued;
            var roundedClean = Math.Round(unroundedClean, 5);
            var roundedAip = roundedClean + roundedAccrued;

            var spotmeasures = new ResultStore();
            spotmeasures.Add(Keys.RoundedAip, roundedAip);
            spotmeasures.Add(Keys.RoundedClean, roundedClean);
            spotmeasures.Add(Keys.UnroundedAip, unroundedAip);
            spotmeasures.Add(Keys.UnroundedClean, unroundedClean);
            spotmeasures.Add(Keys.UnroundedAccrued, unroundedAccrued);
            spotmeasures.Add(Keys.TradingWithNextCoupon, tradingWithNextCoupon ? 1.0 : 0.0);

            BondMeasures.Add(spotmeasures);

            // Compute Risk measures
            var dZ_dy = Math.Pow(v, 3) / 2;
            var dA1_dZ = typicalCoupon * ((1 - (n + 1) * Math.Pow(v, n) + n * Math.Pow(v, n + 1)) / Math.Pow(1 - v, 2)) + n * N * Math.Pow(v, n - 1);
            var d2A1_dZ2 = typicalCoupon * ((2 - n * (n + 1) * Math.Pow(v, n - 1) + 2 * (Math.Pow(n, 2) - 1) * Math.Pow(v, n) - n * (n - 1) * Math.Pow(v, n + 1)) / Math.Pow(1 - v, 3)) + n * (n - 1) * N * Math.Pow(v, n - 2);
            var dA_dZ = dB_dZ * A1 + dA1_dZ * brokenPeriodDf;
            var d2A_dZ2 = d2B_dZ2 * A1 + 2 * dB_dZ * dA1_dZ + d2A1_dZ2 * brokenPeriodDf;
            var dA_dy = -(Math.Pow(v, 2) / 2) * dA_dZ;
            var d2A_dy2 = (Math.Pow(v, 4) / 4) * d2A_dZ2 + (Math.Pow(v, 3) / 2) * dA_dZ;

            var delta = dA_dy/100;
            var randsPerPoint = 100 * delta;
            var modifiedDuration = -dA_dy / unroundedAip;
            var duration = modifiedDuration / v;
            var convexity = d2A_dy2 / unroundedAip;

            var riskmeasures = new ResultStore();
            riskmeasures.Add(Keys.Delta, delta);
            riskmeasures.Add(Keys.RandsPerPoint, randsPerPoint);
            riskmeasures.Add(Keys.ModifiedDuration, modifiedDuration);
            riskmeasures.Add(Keys.Duration, duration);
            riskmeasures.Add(Keys.Convexity, convexity);

            BondMeasures.Add(riskmeasures);

            return BondMeasures;
        }

        public static ResultStore GetSpotMeasures(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var results = GetBondMeasures(bond, settleDate, ytm);
            var spotmeasures = results[0];
            return spotmeasures;
        }

        public static ResultStore GetRiskMeasures(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var results = GetBondMeasures(bond, settleDate, ytm);
            var riskmeasures = results[1];
            return riskmeasures;
        }

        public static class Keys
        {
            public const string RoundedAip = "roundedAip";
            public const string RoundedClean = "roundedClean";
            public const string UnroundedAip = "unroundedAip";
            public const string UnroundedClean = "unroundedClean";
            public const string UnroundedAccrued = "unroundedAccrued";
            public const string TradingWithNextCoupon = "tradingWithNextCoupon";
            public const string Delta = "delta";
            public const string RandsPerPoint = "randsPerPoint";
            public const string ModifiedDuration = "modifiedDuration";
            public const string Duration = "duration";
            public const string Convexity = "convexity";
        }
    }
}