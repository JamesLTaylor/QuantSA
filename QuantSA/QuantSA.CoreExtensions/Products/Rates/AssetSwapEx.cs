using System;
using QuantSA.Shared;
using QuantSA.Shared.Dates;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.MarketObservables;
using System.Collections.Generic;
using QuantSA.Shared.Conventions.DayCount;
using QuantSA.Shared.Conventions.BusinessDay;
using System.Linq;
using QuantSA.Shared.Primitives;
using QuantSA.Core.MarketData;
using QuantSA.Shared.MarketData;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Primitives;


namespace QuantSA.CoreExtensions.Products.Rates
{
    public static class AssetSwapEx
    {
        private static Date GetLastCouponDateOnOrBefore(this AssetSwap assetSwap, Date settleDate)
        {
            var thisYearCpn1 = new Date(settleDate.Year, assetSwap.couponMonth1, assetSwap.couponDay1);
            var thisYearCpn2 = new Date(settleDate.Year, assetSwap.couponMonth2, assetSwap.couponDay2);
            var lastYearCpn2 = new Date(settleDate.Year - 1, assetSwap.couponMonth2, assetSwap.couponDay2);

            if (settleDate > thisYearCpn2)
                return thisYearCpn2;
            if (settleDate > thisYearCpn1)
                return thisYearCpn1;
            return lastYearCpn2;
        }

        private static Date GetNextCouponDate(this AssetSwap assetSwap, Date couponDate)
        {
            if (couponDate.Month == assetSwap.couponMonth2)
                return new Date(couponDate.Year + 1, assetSwap.couponMonth1, assetSwap.couponDay1);
            return new Date(couponDate.Year, assetSwap.couponMonth2, assetSwap.couponDay2);
        }

        public static ResultStore UnitedAssetSwapMeasures(this AssetSwap assetSwap, Date settleDate, double ytm, Date[] discountCurveDates, double[] discountCurveRates, Date[] forecastCurveDates, double[] forecastCurveRates)
        {
            
            //Bond Price Calculations
            var N = 100.0;
            var typicalCoupon = N * assetSwap.fixedRate / 2;
            var t0 = assetSwap.GetLastCouponDateOnOrBefore(settleDate);
            var t1 = assetSwap.GetNextCouponDate(t0);
            var n = (int)Math.Round((assetSwap.maturityDate - t1) / 182.625);
            var tradingWithNextCoupon = t1 - settleDate > assetSwap.booksCloseDateDays;
            var d = tradingWithNextCoupon ? settleDate - t0 : settleDate - t1;
            var unroundedAccrued = N * assetSwap.fixedRate * d / 365.0;
            var roundedAccrued = Math.Round(unroundedAccrued, 5);
            var couponAtT1 = tradingWithNextCoupon ? typicalCoupon : 0.0;
            var v = 1 / (1 + ytm / 2);

            double brokenPeriodDf;
            if (n > 0)
                brokenPeriodDf = Math.Pow(v, ((double)t1 - settleDate) / (t1 - t0));
            else
                brokenPeriodDf = 1 / (1 + ytm * ((double)t1 - settleDate) / 365.0);

            var unroundedAip = brokenPeriodDf *
                               (couponAtT1 + typicalCoupon * v * (1 - Math.Pow(v, n)) / (1 - v) + N * Math.Pow(v, n));

            var unroundedClean = unroundedAip - unroundedAccrued;
            var roundedClean = Math.Round(unroundedClean, 5);
            var roundedAip = roundedClean + roundedAccrued;

            //Create trade date
            var unAdjTradeDate = settleDate.AddDays(-3);
            var tradeDate = BusinessDayStore.ModifiedFollowing.Adjust(unAdjTradeDate, assetSwap.zaCalendar);

            // Create discount and forecast curves
            IDiscountingSource discountCurve = new DatesAndRates(assetSwap.ccy, tradeDate, discountCurveDates, discountCurveRates);
            IFloatingRateSource forecastCurve = new ForecastCurve(tradeDate, assetSwap.index, forecastCurveDates, forecastCurveRates);

            //Create Asset Swap
            var swap = CreateAssetSwap(assetSwap.payFixed, settleDate, assetSwap.maturityDate, assetSwap.index, assetSwap.fixedRate, assetSwap.spread,
                assetSwap.zaCalendar, assetSwap.couponMonth1, assetSwap.couponDay1, assetSwap.couponMonth2, assetSwap.couponDay2, assetSwap.booksCloseDateDays,
                assetSwap.ccy, forecastCurve);

            //Set value date
            swap.SetValueDate(tradeDate);

            //Set index values
            swap.SetIndexValues(assetSwap.index, swap.indexValues1);

            //Calculate present value of fixed and floating cashflows
            var numeratorCFs = swap.GetCFs().PV(discountCurve);

            //Calculate present value of denominator cashflows for spread equation
            var denomCFs = new List<Cashflow>();
            for (var i = 0; i < swap.paymentDatesFloating.Count; i++)
                if (i <= swap.paymentDatesFloating.Count)
                {
                    denomCFs.Add(new Cashflow(swap.paymentDatesFloating[i], -100 * swap.accrualFractions[i], swap.ccy));
                }

            var denominatorCFs = denomCFs.PV(discountCurve);

            var firstCF = new List<Cashflow>();
            for (var i = 0; i < 1; i++)
                if (i <= 1)
                {
                    firstCF.Add(new Cashflow(settleDate, (roundedAip - 100), swap.ccy));
                }
            var pvFirstCF = firstCF.PV(discountCurve);

            //This is the assetSwapSpread calculation
            var assetSwapSpread = (pvFirstCF + numeratorCFs) / denominatorCFs;

            var results = new ResultStore();
            results.Add(Keys.RoundedAip, roundedAip);
            results.Add(Keys.PVFirstCF, pvFirstCF);
            results.Add(Keys.NumeratorCashFlowsPrice, numeratorCFs);
            results.Add(Keys.DenominatorCashFlowsPrice, denominatorCFs);
            results.Add(Keys.AssetSwapSpread, assetSwapSpread);

            return results;
        }

        public static class Keys
        {
            public const string RoundedAip = "roundedAip";
            public const string PVFirstCF = "pvFirstCF";
            public const string NumeratorCashFlowsPrice = "numeratorCFs";
            public const string DenominatorCashFlowsPrice = "denominatorCFs";
            public const string AssetSwapSpread = "assetSwapSpread";
        }

        public static AssetSwap CreateAssetSwap(double payFixed, Date settleDate, Date maturityDate, FloatRateIndex index,
        double fixedRate, double spread, Calendar calendar, int couponMonth1, int couponDay1, int couponMonth2, int couponDay2, int booksCloseDateDays, Currency ccy,
        IFloatingRateSource forecastCurve)
        {

            //Design floating leg inputs
            var dayCount = Actual365Fixed.Instance;
            var unAdjResetDatesFloating = new List<Date>();
            var unAdjPaymentDatesFloating = new List<Date>();
            var resetDatesFloating = new List<Date>();
            var paymentDatesFloating = new List<Date>();
            var accrualFractions = new List<double>();
            var endDate = maturityDate;
            var paymentDateFloating = new Date(endDate);
            var resetDateFloating = paymentDateFloating.SubtractTenor(index.Tenor);
            while (resetDateFloating >= settleDate)
            {
                unAdjPaymentDatesFloating.Add(paymentDateFloating);
                unAdjResetDatesFloating.Add(resetDateFloating);
                resetDatesFloating.Add(BusinessDayStore.ModifiedFollowing.Adjust(resetDateFloating, calendar));
                paymentDatesFloating.Add(BusinessDayStore.ModifiedFollowing.Adjust(paymentDateFloating, calendar));
                accrualFractions.Add(dayCount.YearFraction(BusinessDayStore.ModifiedFollowing.Adjust(resetDateFloating, calendar), BusinessDayStore.ModifiedFollowing.Adjust(paymentDateFloating, calendar)));
                paymentDateFloating = new Date(resetDateFloating);
                resetDateFloating = paymentDateFloating.SubtractTenor(index.Tenor);
            }

            resetDatesFloating.Reverse();
            paymentDatesFloating.Reverse();
            accrualFractions.Reverse();

            resetDatesFloating[0] = new Date(settleDate);
            var firstResetDate = resetDatesFloating.First();
            var firstPaymentDate = paymentDatesFloating.First();
            accrualFractions[0] = dayCount.YearFraction(firstResetDate, firstPaymentDate);

            var notionalsFloating = resetDatesFloating.Select(d => 1e2);
            var floatingIndices = resetDatesFloating.Select(d => index);
            var spreads = resetDatesFloating.Select(d => spread);

            //Design Fixed leg inputs
            var unAdjPaymentDatesFixed = new List<Date>();
            var paymentDatesFixed = new List<Date>();

            var thisYearCpn1 = new Date(settleDate.Year, couponMonth1, couponDay1);
            var thisYearCpn2 = new Date(settleDate.Year, couponMonth2, couponDay2);
            var lastYearCpn2 = new Date(settleDate.Year - 1, couponMonth2, couponDay2);

            Date lcd; //lcd stands for last coupon date
            if (settleDate > thisYearCpn2)
                lcd = new Date(thisYearCpn2.Year, thisYearCpn2.Month, thisYearCpn2.Day);
            if (settleDate > thisYearCpn1)
                lcd = new Date(thisYearCpn1.Year, thisYearCpn1.Month, thisYearCpn1.Day);
            lcd = new Date(lastYearCpn2.Year, lastYearCpn2.Month, lastYearCpn2.Day);

            Date ncd; //ncd stands for next coupon date
            if (lcd.Month == couponMonth2)
                ncd = new Date(lcd.Year + 1, couponMonth1, couponDay1);
            else
                ncd = new Date(lcd.Year, couponMonth2, couponDay2);

            var paymentDateFixed = new Date(ncd.AddTenor(Tenor.FromMonths(6)));

            while (paymentDateFixed <= endDate)
            {
                unAdjPaymentDatesFixed.Add(paymentDateFixed);
                paymentDatesFixed.Add(BusinessDayStore.ModifiedFollowing.Adjust(paymentDateFixed, calendar));
                paymentDateFixed = paymentDateFixed.AddTenor(Tenor.FromMonths(6));
            }

            var notionalsFixed = paymentDatesFixed.Select(d => 1e2);

            //Setting index values
            var indexValues1 = new double[resetDatesFloating.Count];
            for (var i = 0; i < resetDatesFloating.Count; i++)
                indexValues1[i] = forecastCurve.GetForwardRate(resetDatesFloating[i]);

            //create new instance of asset swap
            var assetSwap = new AssetSwap(payFixed, fixedRate, index, resetDatesFloating, paymentDatesFloating, paymentDatesFixed, spread, 
                couponMonth1, couponDay1, couponMonth2, couponDay2, booksCloseDateDays, maturityDate, accrualFractions,notionalsFixed, notionalsFloating,
                calendar, ccy, floatingIndices, indexValues1, spreads);

            return assetSwap;
        }
    }
}
