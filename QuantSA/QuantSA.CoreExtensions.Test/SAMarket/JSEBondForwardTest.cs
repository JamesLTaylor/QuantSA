using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Products.SAMarket;
using QuantSA.CoreExtensions.SAMarket;
using QuantSA.Shared.Dates;
using QuantSA.Solution.Test;

namespace QuantSA.CoreExtensions.Test.SAMarket
{
    [TestClass]
    public class JSEBondForwardTest
    {
        [TestMethod]
        public void TestAllInPrice()
        {
            var settleDate = new Date(2006, 6, 8);
            var maturityDate = new Date(2026, 12, 21);
            var notional = 1000000.0;
            var annualCouponRate = 0.105;
            var couponMonth1 = 6;
            var couponDay1 = 21;
            var couponMonth2 = 12;
            var couponDay2 = 21;
            var booksCloseDateDays = 10;
            var zaCalendar = new Calendar("Test");
            var bondR186 = new BesaJseBond(maturityDate, notional, annualCouponRate, couponMonth1,
                couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR);

            var ytm = 0.0715;
            var results = bondR186.GetSpotMeasures(settleDate, ytm);
            Assert.AreEqual(140.65075443, (double)results.GetScalar(BesaJseBondEx.Keys.UnroundedAip), 1e-8);
            Assert.AreEqual(140.65075, (double)results.GetScalar(BesaJseBondEx.Keys.RoundedAip), 1e-8);
        }

        [TestMethod]
        // Books close dates and associated coupon dates both lie in between settlement and forward date
        public void TestForwardPriceCase1()
        {
            var settleDate = new Date(2006, 6, 8);
            var forwardDate = new Date(2006, 6, 29);
            var maturityDate = new Date(2026, 12, 21);
            var notional = 1000000.0;
            var annualCouponRate = 0.105;
            var couponMonth1 = 6;
            var couponDay1 = 21;
            var couponMonth2 = 12;
            var couponDay2 = 21;
            var booksCloseDateDays = 10;
            var zaCalendar = new Calendar("Test");
            var bondForward = new JSEBondForward(forwardDate, maturityDate, notional, annualCouponRate, couponMonth1,
                couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR);

            var ytm = 0.0715;
            var repo = 0.065;

            var results = bondForward.ForwardPrice(settleDate, ytm, repo);
            Assert.AreEqual(135.91926582, (double)results.GetScalar(JSEBondForwardEx.Keys.ForwardPrice), 1e-8);
        }

        [TestMethod]
        // Books close date lies in between settlement and forward date but associated coupon date comes after forward date
        public void TestForwardPriceCase2()
        {
            var settleDate = new Date(2006, 6, 8);
            var forwardDate = new Date(2006, 6, 15);
            var maturityDate = new Date(2026, 12, 21);
            var notional = 1000000.0;
            var annualCouponRate = 0.105;
            var couponMonth1 = 6;
            var couponDay1 = 21;
            var couponMonth2 = 12;
            var couponDay2 = 21;
            var booksCloseDateDays = 10;
            var zaCalendar = new Calendar("Test");
            var bondForward = new JSEBondForward(forwardDate, maturityDate, notional, annualCouponRate, couponMonth1,
                couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR);

            var ytm = 0.0715;
            var repo = 0.065;

            var results = bondForward.ForwardPrice(settleDate, ytm, repo);
            Assert.AreEqual(135.58168536, (double)results.GetScalar(JSEBondForwardEx.Keys.ForwardPrice), 1e-8);
        }

        [TestMethod]
        // No books close date that lies in between settlement and forward date 
        public void TestForwardPriceCase3()
        {
            var settleDate = new Date(2006, 6, 1);
            var forwardDate = new Date(2006, 6, 8);
            var maturityDate = new Date(2026, 12, 21);
            var notional = 1000000.0;
            var annualCouponRate = 0.105;
            var couponMonth1 = 6;
            var couponDay1 = 21;
            var couponMonth2 = 12;
            var couponDay2 = 21;
            var booksCloseDateDays = 10;
            var zaCalendar = new Calendar("Test");
            var bondForward = new JSEBondForward(forwardDate, maturityDate, notional, annualCouponRate, couponMonth1,
                couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR);

            var ytm = 0.0715;
            var repo = 0.065;

            var results = bondForward.ForwardPrice(settleDate, ytm, repo);
            Assert.AreEqual(140.63595504, (double)results.GetScalar(JSEBondForwardEx.Keys.ForwardPrice), 1e-8);
        }

        [TestMethod]
        // No books close date that lies in between settlement and forward date
        public void TestForwardPriceCase4()
        {
            var settleDate = new Date(2006, 6, 12);
            var forwardDate = new Date(2006, 6, 19);
            var maturityDate = new Date(2026, 12, 21);
            var notional = 1000000.0;
            var annualCouponRate = 0.105;
            var couponMonth1 = 6;
            var couponDay1 = 21;
            var couponMonth2 = 12;
            var couponDay2 = 21;
            var booksCloseDateDays = 10;
            var zaCalendar = new Calendar("Test");
            var bondForward = new JSEBondForward(forwardDate, maturityDate, notional, annualCouponRate, couponMonth1,
                couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR);

            var ytm = 0.0715;
            var repo = 0.065;

            var results = bondForward.ForwardPrice(settleDate, ytm, repo);
            Assert.AreEqual(135.68742401, (double)results.GetScalar(JSEBondForwardEx.Keys.ForwardPrice), 1e-8);
        }

        [TestMethod]
        public void TestSettlementDateGreaterThanForwardDate_ReturnArgumentException()
        {
            var settleDate = new Date(2006, 6, 20);
            var forwardDate = new Date(2006, 6, 19);
            var maturityDate = new Date(2026, 12, 21);
            var notional = 1000000.0;
            var annualCouponRate = 0.105;
            var couponMonth1 = 6;
            var couponDay1 = 21;
            var couponMonth2 = 12;
            var couponDay2 = 21;
            var booksCloseDateDays = 10;
            var zaCalendar = new Calendar("Test");
            var bondForward = new JSEBondForward(forwardDate, maturityDate, notional, annualCouponRate, couponMonth1,
                couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR);

            var ytm = 0.0715;
            var repo = 0.065;

            Assert.ThrowsException<ArgumentException>(() => bondForward.ForwardPrice(settleDate, ytm, repo));
        }

    }
}
