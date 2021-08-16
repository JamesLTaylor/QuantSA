using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Products.SAMarket;
using QuantSA.CoreExtensions.SAMarket;
using QuantSA.Shared.Dates;
using QuantSA.Solution.Test;
using QuantSA.Core.Formulae;

namespace QuantSA.CoreExtensions.Test.SAMarket
{
    [TestClass]
    public class JSEBondOptionTest
    {

        [TestMethod]
        public void TestBondOptionPrice()
        {
            var settleDate = new Date(2008, 1, 17); // Option related
            var maturityDate = new Date(2009, 8, 1); // Bond related
            var forwardDate = new Date(2008, 4, 17); // Forward related
            var notional = 1000000.0; //
            var annualCouponRate = 0.13; // According to JSE Bond Pricer for R153
            var couponMonth1 = 2; //
            var couponDay1 = 28; //
            var couponMonth2 = 8; //
            var couponDay2 = 31; //
            var booksCloseDateDays = 10; //
            var zaCalendar = new Calendar("Test"); //
            var bondOptionR153 = new JSEBondOption(forwardDate, maturityDate, PutOrCall.Call, settleDate); //

            var bondForwardR153 = new JSEBondForward(forwardDate, maturityDate, notional, annualCouponRate, couponMonth1,
            couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR); 

            var ytm = 0.0930; //
            var repo = 0.1075; // See example
            var strike = 0.09; //
            var vol = 0.07; //

            var results = JSEBondOptionEx.BlackOption(bondOptionR153, strike, vol, repo, bondForwardR153, ytm);
            Assert.AreEqual(103.85470134714154, (double)results.GetScalar(JSEBondOptionEx.Keys.BlackOption), 1e-8);
        }

        [TestMethod]
        public void TestAllInPrice()
        {
            var settleDate = new Date(2008, 1, 17);
            var maturityDate = new Date(2009, 8, 1);
            var notional = 1000000.0;
            var annualCouponRate = 0.13;
            var couponMonth1 = 2;
            var couponDay1 = 28;
            var couponMonth2 = 8;
            var couponDay2 = 31;
            var booksCloseDateDays = 10;
            var zaCalendar = new Calendar("Test");
            var bondR153 = new BesaJseBond(maturityDate, notional, annualCouponRate, couponMonth1,
                couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR);

            var ytm = 0.0930;

            var results = bondR153.GetSpotMeasures(settleDate, ytm);
            Assert.AreEqual(110.400707433627, (double)results.GetScalar(BesaJseBondEx.Keys.UnroundedAip), 1e-8);
            Assert.AreEqual(110.4007, (double)results.GetScalar(BesaJseBondEx.Keys.RoundedAip), 1e-8);
        }

        [TestMethod]
        public void TestBondForwardPrice()
        {
            var settleDate = new Date(2008, 1, 17);
            var forwardDate = new Date(2008, 4, 17);
            var maturityDate = new Date(2009, 8, 1);
            var notional = 1000000.0;
            var annualCouponRate = 0.13;
            var couponMonth1 = 2;
            var couponDay1 = 28;
            var couponMonth2 = 8;
            var couponDay2 = 31;
            var booksCloseDateDays = 10;
            var zaCalendar = new Calendar("Test");
            var bondForward = new JSEBondForward(forwardDate, maturityDate, notional, annualCouponRate, couponMonth1,
                couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR);

            var ytm = 0.0930;
            var repo = 0.1075;

            var results = bondForward.ForwardPrice(settleDate, ytm, repo);
            Assert.AreEqual(106.7657852, (double)results.GetScalar(JSEBondForwardEx.Keys.ForwardPrice), 1e-8);
        }
    }
}

