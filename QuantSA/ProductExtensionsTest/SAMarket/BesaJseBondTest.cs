using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General.Products.SAMarket;
using QuantSA.General;
using QuantSA.General.Dates;
using QuantSA.ProductExtensions.SAMarket;

namespace ProductExtensionsTest.SAMarket
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
            Calendar zaCalendar = new Calendar();
            var bondR2030 = new BesaJseBond(maturityDate, notional, annualCouponRate, couponMonth1, 
                couponDay1, couponMonth2, couponDay2, zaCalendar);

            double ytm = 0.097;
            ResultStore results = bondR2030.GetSpotMeasures(settleDate, ytm);
            Assert.AreEqual(87.85607808, results.GetScalar(BesaJseBondEx.Keys.unroundedAip),1e-8);
            Assert.AreEqual(87.85608, results.GetScalar(BesaJseBondEx.Keys.roundedAip), 1e-8);
            Assert.AreEqual(87.15470822, results.GetScalar(BesaJseBondEx.Keys.unroundedClean), 1e-8);
            Assert.AreEqual(87.15471, results.GetScalar(BesaJseBondEx.Keys.roundedClean), 1e-8);
            Assert.AreEqual(0.701369836, results.GetScalar(BesaJseBondEx.Keys.unroundedAccrued), 1e-7);
        }
    }
}
