using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Shared.Dates;
using QuantSA.Solution.Test;
using QuantSA.CoreExtensions.Products.Rates;
using QuantSA.Core.Products.Rates;
using QuantSA.Core.Dates;

using QuantSA.Shared.MarketObservables;
using System.Linq;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.MarketData;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Primitives;
using QuantSA.Core.MarketData;
using QuantSA.CoreExtensions.ProductPVs.Rates;


namespace QuantSA.CoreExtensions.Test.SAMarket
{

    [TestClass]

    public class AssetSwapTest
    {
        [TestMethod]
        public void TestAssetSwap()  ///There is a lot going on here and this needs to be cleaned up.
        {
            //Bond inputs 
            var settleDate = new Date(2013, 3, 8);
            var maturityDate = new Date(2015, 9, 15);
            var notional = 100000000.0;
            var annualCouponRate = 0.135;
            var couponMonth1 = 3;
            var couponDay1 = 15;
            var couponMonth2 = 9;
            var couponDay2 = 15;
            var booksCloseDateDays = 10;
            
            var ytm = 0.05757;
            var indexFloating = "ZAR.JIBAR.3M";

            var tradeDate = settleDate.AddDays(-3);
            var payFixed = -1;

            //Testing
            var ccy = new Currency("ZAR");
            var ncd = new Date(2013, 3, 15);
            var indexFixedLeg = new FloatRateIndex("ZAR.JIBAR.6M", ccy, "JIBAR", Tenor.FromMonths(6));
            var indexFloatLeg = new FloatRateIndex("ZAR.JIBAR.3M", ccy, "JIBAR", Tenor.FromMonths(3));
            var nDays = 365;
            var extra = 1;
            var x = ((maturityDate - settleDate) / nDays) + extra;
            var tenor = Tenor.FromYears(x);

            Date[] holidays =
{
                new Date(2013, 1, 1), new Date(2013, 3, 21), new Date(2013, 3, 29), new Date(2013, 4, 1), new Date(2013, 4, 27), new Date(2013, 5, 1),
                new Date(2013, 6, 17), new Date(2013, 8, 9), new Date(2013, 9, 24), new Date(2013, 12, 16), new Date(2013, 12, 25), new Date(2013, 12, 26),
                new Date(2014, 1, 1), new Date(2014, 3, 21), new Date(2014, 4, 18), new Date(2014, 4, 21), new Date(2014, 4, 28), new Date(2014, 5, 1),
                new Date(2014, 5, 7), new Date(2014, 6, 16), new Date(2014, 8, 9), new Date(2014, 9, 24), new Date(2014, 12, 16), new Date(2014, 12, 25),
                new Date(2014, 12, 26), new Date(2015, 1, 1), new Date(2015, 3, 21), new Date(2015, 4, 3), new Date(2015, 4, 6), new Date(2015, 4, 27),
                new Date(2015, 5, 1), new Date(2015, 6, 16), new Date(2015, 8, 10), new Date(2015, 9, 24), new Date(2015, 12, 16), new Date(2020, 12, 25),
                new Date(2020, 12, 26)
            };

            var zaCalendar = new Calendar("Test", holidays);

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

            IMarketDataContainer marketData = new MarketDataContainer();
            IFloatingRateSource _forecastCurve = marketData.Get(new FloatingRateSourceDescription(indexFloatLeg));
            IDiscountingSource _discountCurve = marketData.Get(new DiscountingSourceDescription(TestHelpers.ZAR));

            IDiscountingSource discountCurve = new DatesAndRates(ccy, tradeDate, datesLong, ratesLong);
            IFloatingRateSource forecastCurve = new ForecastCurve(tradeDate, indexFloatLeg, datesLong2, ratesLong2);

            var ASWfixedLeg = CreateFixedLegASW(payFixed, ncd, maturityDate, tenor, indexFixedLeg, annualCouponRate, zaCalendar);
            ASWfixedLeg.SetValueDate(tradeDate);

            var fixedcash = ASWfixedLeg.GetCFs().PV(discountCurve);

            var spread = 0;
            var ASWfloatLeg = CreateFloatLegASW(payFixed, settleDate, maturityDate, tenor, indexFloatLeg, spread, zaCalendar);
            ASWfloatLeg.SetValueDate(tradeDate);

            var floatingcash = ASWfloatLeg.CurvePV1(forecastCurve, discountCurve);


            //Create new instance of assetSwap

            var newSwap = new AssetSwap(payFixed, indexFloating, maturityDate, notional, annualCouponRate, couponMonth1, couponDay1,
                couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR, datesLong, ratesLong, datesLong2, ratesLong2);

            // Get assetSwap measures

            var bondprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.RoundedAip);
            var fixedprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.FixedCashFlowsPrice);
            var floatingprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.FloatingCashFlowsPrice);
            var denominatorprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.DenominatorCashFlowsPrice);
            var assetSwapSpread = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.AssetSwapSpread);

        }

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

        public static FixedLegASW CreateFixedLegASW(double payFixed, Date nextCouponDateCalibrationDate, Date maturityDate, Tenor tenor, FloatRateIndex index,
        double fixedRate, Calendar calendar)
        {
            DateGenerators.CreateDatesASWFixed(nextCouponDateCalibrationDate, maturityDate, tenor, index.Tenor, out var resetDates,
                out var paymentDates, calendar);
            var notionals = resetDates.Select(d => 1e2);
            var rates = resetDates.Select(d => fixedRate);
            return new FixedLegASW(payFixed, index.Currency, paymentDates, notionals, rates);

        }
    }
}
