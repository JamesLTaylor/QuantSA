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
        public enum OptionPriceandGreeks
        {
            UnroundedAIP,
            RoundedAIP,
            UnroundedClean,
            RoundedClean,
            UnroundedAccrued,
            RoundedAccrued,
            Delta,
            RandsPerPoint,
            ModifiedDuration,
            Duration,
            Convexity
        }
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
        /// <returns>the unrounded accrued accrued interest</returns>
        public static double UnroundedAccruedInterest(this BesaJseBond bond, Date settleDate)
        {
            var N = 100.0;
            var t0 = bond.GetLastCouponDateOnOrBefore(settleDate);
            var t1 = bond.GetNextCouponDate(t0);
            var tradingWithNextCoupon = t1 - settleDate > bond.booksCloseDateDays;
            var d = tradingWithNextCoupon ? settleDate - t0 : settleDate - t1;
            var unroundedAccrued = N * bond.annualCouponRate * d / 365.0;

            return unroundedAccrued;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns>the rounded accrued interest</returns>
        public static double RoundedAccruedInterest(this BesaJseBond bond, Date settleDate)
        {
            return Math.Round(bond.UnroundedAccruedInterest(settleDate), 5);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns>the unrounded all-in price</returns>
        public static double UnroundedAIP(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var N = 100.0;
            var typicalCoupon = N * bond.annualCouponRate / 2;
            var t0 = bond.GetLastCouponDateOnOrBefore(settleDate);
            var t1 = bond.GetNextCouponDate(t0);
            var tradingWithNextCoupon = t1 - settleDate > bond.booksCloseDateDays;
            var d = tradingWithNextCoupon ? settleDate - t0 : settleDate - t1;

            var couponAtT1 = tradingWithNextCoupon ? typicalCoupon : 0.0;
            var v = 1 / (1 + ytm / 2);

            var n = (int)Math.Round((bond.maturityDate - t1) / 182.625);

            double brokenPeriod, brokenPeriodDf;
            if (n > 0)
            {
                brokenPeriod = ((double)t1 - settleDate) / (t1 - t0);
                brokenPeriodDf = Math.Pow(v, brokenPeriod);
            }
            else
            {
                brokenPeriod = ((double)t1 - settleDate) / 182.25;
                brokenPeriodDf = 1 / (1 + ytm * brokenPeriod / 2);
            }

            var A1 = (couponAtT1 + typicalCoupon * v * (1 - Math.Pow(v, n)) / (1 - v) + N * Math.Pow(v, n));
            var unroundedAip = brokenPeriodDf * A1;

            return unroundedAip;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns>the rounded all-in price</returns>
        public static double RoundedAIP(this BesaJseBond bond, Date settleDate, double ytm)
        {
            return Math.Round(bond.UnroundedAIP(settleDate, ytm), 5);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns>the unrounded clean price</returns>
        public static double UnroundedClean(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var unroundedClean = bond.UnroundedAIP(settleDate, ytm) - bond.UnroundedAccruedInterest(settleDate);
            return unroundedClean;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns>the rounded clean price</returns>
        public static double RoundedClean(this BesaJseBond bond, Date settleDate, double ytm)
        {
            return Math.Round(bond.UnroundedClean(settleDate, ytm), 5);
        }

        private static double[] DeltaAndConvexity(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var N = 100.0;
            var typicalCoupon = N * bond.annualCouponRate / 2;
            var t0 = bond.GetLastCouponDateOnOrBefore(settleDate);
            var t1 = bond.GetNextCouponDate(t0);
            var v = 1 / (1 + ytm / 2);
            var n = (int)Math.Round((bond.maturityDate - t1) / 182.625);

            double brokenPeriod, brokenPeriodDf, dB_dZ;
            double d2B_dZ2 = 0;
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
            }

            var A1 = bond.UnroundedAIP(settleDate, ytm) / brokenPeriodDf;
            var dA1_dZ = typicalCoupon * ((1 - (n + 1) * Math.Pow(v, n) + n * Math.Pow(v, n + 1)) / Math.Pow(1 - v, 2)) + n * N * Math.Pow(v, n - 1);
            var dA_dZ = dB_dZ * A1 + dA1_dZ * brokenPeriodDf;
            var d2A1_dZ2 = typicalCoupon * ((2 - n * (n + 1) * Math.Pow(v, n - 1) + 2 * (Math.Pow(n, 2) - 1) * Math.Pow(v, n) - n * (n - 1) * Math.Pow(v, n + 1)) / Math.Pow(1 - v, 3)) + n * (n - 1) * N * Math.Pow(v, n - 2);
            var d2A_dZ2 = d2B_dZ2 * A1 + 2 * dB_dZ * dA1_dZ + d2A1_dZ2 * brokenPeriodDf;
            var dA_dy = -(Math.Pow(v, 2) / 2) * dA_dZ;
            var d2A_dy2 = (Math.Pow(v, 4) / 4) * d2A_dZ2 + (Math.Pow(v, 3) / 2) * dA_dZ;
            var delta = dA_dy / 100;
            var convexity = d2A_dy2 / bond.UnroundedAIP(settleDate, ytm);

            double[] measures = new double[2];
            measures[0] = delta;
            measures[1] = convexity;

            return measures;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns>delta of the bond</returns>
        public static double Delta(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var delta = bond.DeltaAndConvexity(settleDate, ytm)[0];
            return delta;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns>rand per points of the bond</returns>
        public static double RandsPerPoint(this BesaJseBond bond, Date settleDate, double ytm)
        {
            return 100 * bond.Delta(settleDate, ytm);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns>modified duration of the bond</returns>
        public static double ModefiedDuration(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var dA_dy = bond.RandsPerPoint(settleDate, ytm);
            return -dA_dy / bond.UnroundedAIP(settleDate, ytm);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns>duration of the bond</returns>
        public static double Duration(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var v = 1 / (1 + ytm / 2);
            return bond.ModefiedDuration(settleDate, ytm) / v;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bond"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <returns>convexity of the bond</returns>
        public static double Convexity(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var convexity = bond.DeltaAndConvexity(settleDate, ytm)[1];
            return convexity;
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
            var spotmeasures = new ResultStore();

            spotmeasures.Add(Keys.RoundedAip, bond.RoundedAIP(settleDate, ytm));
            spotmeasures.Add(Keys.RoundedClean, bond.RoundedClean(settleDate, ytm));
            spotmeasures.Add(Keys.UnroundedAip, bond.UnroundedAIP(settleDate, ytm));
            spotmeasures.Add(Keys.UnroundedClean, bond.UnroundedClean(settleDate, ytm));
            spotmeasures.Add(Keys.UnroundedAccrued, bond.UnroundedAccruedInterest(settleDate));

            return spotmeasures;
        }

        public static ResultStore GetRiskMeasures(this BesaJseBond bond, Date settleDate, double ytm)
        {
            var riskmeasures = new ResultStore();

            riskmeasures.Add(Keys.Delta, bond.Delta(settleDate, ytm));
            riskmeasures.Add(Keys.RandsPerPoint, bond.RandsPerPoint(settleDate, ytm));
            riskmeasures.Add(Keys.Duration, bond.Duration(settleDate, ytm));
            riskmeasures.Add(Keys.ModifiedDuration, bond.ModefiedDuration(settleDate, ytm));
            riskmeasures.Add(Keys.Convexity, bond.Convexity(settleDate, ytm));

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