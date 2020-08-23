using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Products.SAMarket;
using QuantSA.CoreExtensions.SAMarket;
using QuantSA.Shared;
using QuantSA.Shared.Dates;
using QuantSA.Solution.Test;

namespace QuantSA.CoreExtensions.Test.SAMarket
{
    [TestClass]
    public class BesaJseBondTest
    {
        [TestMethod]
        public void TestAllInPrice()
        {
            Date settleDate = new Date(2016, 3, 3);
            Date maturityDate = new Date(2030, 1, 31);
            double notional = 1000000;
            double annualCouponRate = 0.08;
            int couponMonth1 = 1;
            int couponDay1 = 31;
            int couponMonth2 = 7;
            int couponDay2 = 31;
            Calendar zaCalendar = new Calendar("Test");
            var bondR2030 = new BesaJseBond(maturityDate, notional, annualCouponRate, couponMonth1, 
                couponDay1, couponMonth2, couponDay2, zaCalendar, TestHelpers.ZAR);

            double ytm = 0.097;
            ResultStore results = bondR2030.GetSpotMeasures(settleDate, ytm);
            Assert.AreEqual(87.85607808, (double)results.GetScalar(BesaJseBondEx.Keys.UnroundedAip),1e-8);
            Assert.AreEqual(87.85608, (double)results.GetScalar(BesaJseBondEx.Keys.RoundedAip), 1e-8);
            Assert.AreEqual(87.15470822, (double)results.GetScalar(BesaJseBondEx.Keys.UnroundedClean), 1e-8);
            Assert.AreEqual(87.15471, (double)results.GetScalar(BesaJseBondEx.Keys.RoundedClean), 1e-8);
            Assert.AreEqual(0.701369836, (double)results.GetScalar(BesaJseBondEx.Keys.UnroundedAccrued), 1e-7);
        }
    }
}
