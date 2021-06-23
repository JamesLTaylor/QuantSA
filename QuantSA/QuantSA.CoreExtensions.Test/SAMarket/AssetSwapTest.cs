﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Shared.Dates;
using QuantSA.CoreExtensions.Products.Rates;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.Primitives;

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
            var payFixed = -1;
            var ccy = new Currency("ZAR");

            var tradeDate = settleDate.AddDays(-3);

     
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

            double[] ratesLong = { 0.0476968834358959, 0.0501430754329056, 0.0509218045201335, 0.0507490287827958, 0.0506113173158310, 0.0506165682297636,0.0507674467276694,0.0510331587565828, 0.0513967654284081, 0.0518952383980752,
            0.0524896820133486, 0.0530856475345829, 0.0536795171239116, 0.0542763619213055, 0.0550232983685124, 0.0557743840043158, 0.0565215576596013, 0.0572650196269946, 0.0580081291489062, 0.0587559589725926, 0.0595015505438427, 0.0602449665660311,
            0.0609574950515419, 0.0616761490635943, 0.0623935543507907, 0.0631097408589442, 0.0637906604035856, 0.0644782668196503, 0.0651654909303646, 0.0658597518938604};
            
            Date[] datesLong2 =
{
                new Date(2013, 3, 8), new Date(2013, 6, 18), new Date(2013, 9, 16), new Date(2013, 12, 17), new Date(2014, 3, 17), new Date(2014, 6, 17),
                new Date(2014, 9, 15), new Date(2014, 12, 15), new Date(2015, 3, 16), new Date(2015, 6, 15),
            };

            double[] ratesLong2 = { 0.0513516962584837, 0.0508560960486011, 0.0506778350115049, 0.0510319212391443, 0.0518172259443010, 0.0528264747654130, 0.0541426832758913,
            0.0560265891867828, 0.0577427520443597, 0.0588585700038349};

            //Create new instance of assetSwap

            var newSwap = new AssetSwap(payFixed, indexFloating, maturityDate, notional, annualCouponRate, couponMonth1, couponDay1,
                couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, ccy, datesLong, ratesLong, datesLong2, ratesLong2);

            // Get assetSwap measures

            var bondprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.RoundedAip);
            var fixedprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.FixedCashFlowsPrice);
            var floatingprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.FloatingCashFlowsPrice);
            var denominatorprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.DenominatorCashFlowsPrice);
            var assetSwapSpread = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.AssetSwapSpread);

        }
    }
}
