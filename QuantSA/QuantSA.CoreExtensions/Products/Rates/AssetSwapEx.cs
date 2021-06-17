using System;
using QuantSA.Shared;
using QuantSA.Shared.Dates;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Core.Dates;
using System.Linq;
using QuantSA.Shared.MarketData;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Primitives;
using QuantSA.Core.MarketData;
using QuantSA.CoreExtensions.ProductPVs.Rates;


namespace QuantSA.CoreExtensions.Products.Rates
{

    public static class AssetSwapEx
    {
        private static Date GetLastCouponDateOnOrBefore(this AssetSwap assetSwap, Date settleDate)
        {
            var thisYearCpn1 = new Date(settleDate.Year, assetSwap.underlyingBond.couponMonth1, assetSwap.underlyingBond.couponDay1);
            var thisYearCpn2 = new Date(settleDate.Year, assetSwap.underlyingBond.couponMonth2, assetSwap.underlyingBond.couponDay2);
            var lastYearCpn2 = new Date(settleDate.Year - 1, assetSwap.underlyingBond.couponMonth2, assetSwap.underlyingBond.couponDay2);

            if (settleDate > thisYearCpn2)
                return thisYearCpn2;
            if (settleDate > thisYearCpn1)
                return thisYearCpn1;
            return lastYearCpn2;
        }

        private static Date GetNextCouponDate(this AssetSwap assetSwap, Date couponDate)
        {
            if (couponDate.Month == assetSwap.underlyingBond.couponMonth2)
                return new Date(couponDate.Year + 1, assetSwap.underlyingBond.couponMonth1, assetSwap.underlyingBond.couponDay1);
            return new Date(couponDate.Year, assetSwap.underlyingBond.couponMonth2, assetSwap.underlyingBond.couponDay2);
        }

        public static ResultStore AssetSwapMeasures(this AssetSwap assetSwap, Date settleDate, double ytm)
        {

            //Bond Price Calculations
            var N = 100.0;
            var typicalCoupon = N * assetSwap.underlyingBond.annualCouponRate / 2;
            var t0 = assetSwap.GetLastCouponDateOnOrBefore(settleDate);
            var t1 = assetSwap.GetNextCouponDate(t0);
            var n = (int)Math.Round((assetSwap.underlyingBond.maturityDate - t1) / 182.625);
            var tradingWithNextCoupon = t1 - settleDate > assetSwap.underlyingBond.booksCloseDateDays;
            var d = tradingWithNextCoupon ? settleDate - t0 : settleDate - t1;
            var unroundedAccrued = N * assetSwap.underlyingBond.annualCouponRate * d / 365.0;
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


            //Creating FixedLeg and FloatingLeg of assetSwap
            var nDays = 365;
            var extra = 1;
            var x = ((assetSwap.underlyingBond.maturityDate - settleDate) / nDays) + extra;
            var tenor = Tenor.FromYears(x);

            var tradeDate = settleDate.AddDays(-3);

            var ccy = new Currency("ZAR");
            var indexFixedLeg = new FloatRateIndex("ZAR.JIBAR.6M", ccy, "JIBAR", Tenor.FromMonths(6));
            var indexFloatLeg = new FloatRateIndex(assetSwap.RateIndex, ccy, "JIBAR", Tenor.FromMonths(assetSwap.tenorfloat));

            var spread = 0.0;

            var ASWfixedLeg = CreateFixedLegASW(assetSwap.payFixed, t1, assetSwap.underlyingBond.maturityDate, tenor, indexFixedLeg, assetSwap.underlyingBond.annualCouponRate, assetSwap.zaCalendar);
            ASWfixedLeg.SetValueDate(tradeDate);

            var ASWfloatLeg = CreateFloatLegASW(assetSwap.payFixed, settleDate, assetSwap.underlyingBond.maturityDate, tenor, indexFloatLeg, spread, assetSwap.zaCalendar);
            ASWfloatLeg.SetValueDate(tradeDate);

            // Create discount and forecast curves

            IDiscountingSource discountCurve = new DatesAndRates(ccy, tradeDate, assetSwap.discountCurveDates, assetSwap.discountCurveRates);
            IFloatingRateSource forecastCurve = new ForecastCurve(tradeDate, indexFloatLeg, assetSwap.forecastCurveDates, assetSwap.forecastCurveRates);

            // Calculate PV of fixed and floating cashflows based on discount and forecast curves above

            var fixedCashFlowsPrice = ASWfixedLeg.GetCFs().PV(discountCurve);
            var floatingCashFlowsPrice = ASWfloatLeg.CurvePV1(forecastCurve, discountCurve);

            // this is to get the denominator in for the spread calculaton that excludes the rates in the PV calcs (CurvePV2)
            var denominatorCashFlowsPrice = ASWfloatLeg.CurvePV2(forecastCurve, discountCurve) * -1;

            //This is the assetSwapSprad
            var assetSwapSpread = ((roundedAip - 100) + (fixedCashFlowsPrice) + (floatingCashFlowsPrice)) / denominatorCashFlowsPrice;

            //This is to store the results in the ResultStore
            var results = new ResultStore();
            results.Add(Keys.RoundedAip, roundedAip);
            results.Add(Keys.RoundedClean, roundedClean);
            results.Add(Keys.UnroundedAip, unroundedAip);
            results.Add(Keys.UnroundedClean, unroundedClean);
            results.Add(Keys.UnroundedAccrued, unroundedAccrued);
            results.Add(Keys.TradingWithNextCoupon, tradingWithNextCoupon ? 1.0 : 0.0);
            results.Add(Keys.FixedCashFlowsPrice, fixedCashFlowsPrice);
            results.Add(Keys.FloatingCashFlowsPrice, floatingCashFlowsPrice);
            results.Add(Keys.DenominatorCashFlowsPrice, denominatorCashFlowsPrice);
            results.Add(Keys.AssetSwapSpread, assetSwapSpread);

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
            public const string FixedCashFlowsPrice = "fixedCashFlowsPrice";
            public const string FloatingCashFlowsPrice = "floatingCashFlowsPrice";
            public const string DenominatorCashFlowsPrice = "denominatorCashFlowsPrice";
            public const string AssetSwapSpread = "assetSwapSpread";
        }

        //Adding the creation of a fixed leg for ASW
        public static FixedLegASW CreateFixedLegASW(double payFixed, Date nextCouponDateCalibrationDate, Date maturityDate, Tenor tenor, FloatRateIndex index,
        double fixedRate, Calendar calendar)
        {
            DateGenerators.CreateDatesASWFixed(nextCouponDateCalibrationDate, maturityDate, tenor, index.Tenor, out var resetDates,
                out var paymentDates, calendar);
            var notionals = resetDates.Select(d => 1e2);
            var rates = resetDates.Select(d => fixedRate);
            return new FixedLegASW(payFixed, index.Currency, paymentDates, notionals, rates);
        }

        //Adding the creation of a floating leg for ASW
        public static FloatLegASW CreateFloatLegASW(double payFixed, Date calibrationDate, Date maturityDate, Tenor tenor, FloatRateIndex index,
        double spread, Calendar calendar)
        {
            DateGenerators.CreateDatesASWfloat(calibrationDate, maturityDate, tenor, index.Tenor, out var resetDates,
                out var paymentDates, out var accrualFractions, calendar);
            var notionals = resetDates.Select(d => 1e2);
            var floatingIndices = resetDates.Select(d => index);
            var spreads = resetDates.Select(d => spread);
            return new FloatLegASW(payFixed, index.Currency, paymentDates, notionals, resetDates, floatingIndices, spreads, accrualFractions);
        }
    }
}
