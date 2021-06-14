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

            var ASWfixedLeg = CreateFixedLegASW(assetSwap.payFixed, t1, assetSwap.underlyingBond.maturityDate, tenor, indexFixedLeg, assetSwap.underlyingBond.annualCouponRate);
            ASWfixedLeg.SetValueDate(tradeDate);

            var ASWfloatLeg = CreateFloatLegASW(assetSwap.payFixed, settleDate, assetSwap.underlyingBond.maturityDate, tenor, indexFloatLeg, spread);
            ASWfloatLeg.SetValueDate(tradeDate);

            // Create dates and rates for the discounting and forecasting curves

            Date[] datesLong =
            {
                new Date(2013, 3, 6), new Date(2013, 4, 5), new Date(2013, 6, 5), new Date(2013, 9, 5), new Date(2013, 12, 5), new Date(2014, 3, 5),
                new Date(2014, 6, 5), new Date(2014, 9, 5), new Date(2014, 12, 5), new Date(2015, 3, 5), new Date(2015, 6, 5), new Date(2015, 9, 7),
                new Date(2015, 12, 7), new Date(2016, 3, 7), new Date(2016, 6, 6), new Date(2016, 9, 5), new Date(2016, 12, 5), new Date(2017, 3, 6),
                new Date(2017, 6, 5), new Date(2017, 9, 5), new Date(2017, 12, 5), new Date(2018, 3, 5), new Date(2018, 6, 5), new Date(2018, 9, 5),
                new Date(2018, 12, 5), new Date(2019, 3, 5), new Date(2019, 6, 5), new Date(2019, 9, 5), new Date(2019, 12, 5), new Date(2020, 3, 5),
            };


            double[] ratesLong = { 0.047697, 0.050143, 0.050922, 0.050749, 0.050611, 0.050617, 0.050767, 0.051033, 0.051397, 0.051895,
            0.052490, 0.053086, 0.053680, 0.054276, 0.055023, 0.055774, 0.056522, 0.057265, 0.058008, 0.058756, 0.059502, 0.060245,
            0.060957, 0.061676, 0.062394, 0.063110, 0.063791, 0.064478, 0.065165, 0.065860};

            Date[] datesLong2 =
{
                new Date(2013, 3, 8), new Date(2013, 6, 18), new Date(2013, 9, 16), new Date(2013, 12, 17), new Date(2014, 3, 17), new Date(2014, 6, 17),
                new Date(2014, 9, 15), new Date(2014, 12, 15), new Date(2015, 3, 16), new Date(2015, 6, 15),
            };


            double[] ratesLong2 = { 0.051351696, 0.050856096, 0.050677835, 0.051031921, 0.051817226, 0.052826475, 0.054142683,
            0.056026589, 0.057742752, 0.05885857};

            // Create discount and forecast curves from dates and rate above

            IDiscountingSource discountCurve = new DatesAndRates(ccy, tradeDate, datesLong, ratesLong);
            IFloatingRateSource forecastCurve = new ForecastCurve(tradeDate, indexFloatLeg, datesLong2, ratesLong2);

            // Calculate PV of fixed and floating cashflows based on discount and forecast curves above

            var fixedCashFlowsPrice = ASWfixedLeg.GetCFs().PV(discountCurve) / 1000000;
            var floatingCashFlowsPrice = ASWfloatLeg.CurvePV1(forecastCurve, discountCurve) / 1000000;

            // this is to get the denominator in for the spread calculaton that excludes the rates in the PV calcs (CurvePV2)
            var denominatorCashFlowsPrice = ASWfloatLeg.CurvePV2(forecastCurve, discountCurve) * -100 / assetSwap.underlyingBond.notional;

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
        double fixedRate)
        {
            DateGenerators.CreateDatesNoHolidaysASWFixed(nextCouponDateCalibrationDate, maturityDate, tenor, index.Tenor, out var resetDates,
                out var paymentDates);
            var notionals = resetDates.Select(d => 1e8);
            var rates = resetDates.Select(d => fixedRate);
            return new FixedLegASW(payFixed, index.Currency, paymentDates, notionals, rates);
        }

        //Adding the creation of a floating leg for ASW
        public static FloatLegASW CreateFloatLegASW(double payFixed, Date calibrationDate, Date maturityDate, Tenor tenor, FloatRateIndex index,
        double spread)
        {
            DateGenerators.CreateDatesNoHolidaysASWfloat(calibrationDate, maturityDate, tenor, index.Tenor, out var resetDates,
                out var paymentDates, out var accrualFractions);
            var notionals = resetDates.Select(d => 1e8);
            var floatingIndices = resetDates.Select(d => index);
            var spreads = resetDates.Select(d => spread);
            return new FloatLegASW(payFixed, index.Currency, paymentDates, notionals, resetDates, floatingIndices, spreads, accrualFractions);
        }
    }
}
