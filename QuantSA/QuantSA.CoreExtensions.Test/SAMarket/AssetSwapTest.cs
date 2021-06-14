using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Shared.Dates;
using QuantSA.Solution.Test;
using QuantSA.CoreExtensions.Products.Rates;
using QuantSA.Core.Products.Rates;


namespace QuantSA.CoreExtensions.Test.SAMarket
{
    [TestClass]
    public class AssetSwapTest
    {
        [TestMethod]
        public void TestAssetSwap()
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
            var zaCalendar = new Calendar("Test");
            var ytm = 0.05757;
            var indexFloating = "ZAR.JIBAR.3M";

            var tradeDate = settleDate.AddDays(-3);
            var payFixed = -1;

            //Create new instance of assetSwap

            var newSwap = new AssetSwap(payFixed, indexFloating, maturityDate, notional, annualCouponRate, couponMonth1, couponDay1,
                couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR);

            // Get assetSwap measures

            var bondprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.RoundedAip);
            var fixedprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.FixedCashFlowsPrice);
            var floatingprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.FloatingCashFlowsPrice);
            var denominatorprice = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.DenominatorCashFlowsPrice);
            var assetSwapSpread = newSwap.AssetSwapMeasures(settleDate, ytm).GetScalar(AssetSwapEx.Keys.AssetSwapSpread);

        }
    }
}
