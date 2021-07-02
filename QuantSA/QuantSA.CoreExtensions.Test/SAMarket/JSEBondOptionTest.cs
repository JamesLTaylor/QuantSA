using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Products.SAMarket;
using QuantSA.CoreExtensions.SAMarket;
using QuantSA.Shared.Dates;
using QuantSA.Shared;
using QuantSA.Solution.Test;
using QuantSA.Core.Formulae;

namespace QuantSA.CoreExtensions.Test.SAMarket
{
    [TestClass]
    class JSEBondOptionTest
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
            var bondForwardR153 = new JSEBondForward(forwardDate, maturityDate, notional, annualCouponRate, couponMonth1,
            couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, TestHelpers.ZAR); //

            var ytm = 0.0930; //
            var repo = 10.75; // See example - Don't know what is going on here aka what the value should be
            var strike = 1100555.9; //
            var vol = 0.07; //

            var forwardprice = bondForwardR153.ForwardPrice(settleDate, ytm, repo);
            var bondforwardprice = (double)forwardprice.GetScalar(JSEBondForwardEx.Keys.ForwardPrice);

            var results = JSEBondOptionEx.BlackOption(bondForwardR153, PutOrCall.Call, strike, vol, bondforwardprice, repo);
            Assert.AreEqual(13826.13, (double)results.GetScalar(JSEBondOptionEx.Keys.BlackOption), 1e-8);
        }
    }
}

