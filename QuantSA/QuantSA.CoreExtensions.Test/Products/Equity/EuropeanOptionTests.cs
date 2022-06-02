using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Shared.Dates;
using QuantSA.Solution.Test;
using QuantSA.CoreExtensions.Products.Equity;
using QuantSA.Core.Products.Equity;
using QuantSA.Shared.MarketObservables;
using QuantSA.Core.Formulae;

namespace QuantSA.CoreExtensions.Test.Products.Equity
{
    [TestClass]
    public class EuropeanOptionTests
    {
        [TestMethod]
        public void TestEuropeanOpricePriceAndGreeks()
        {
            double strike = 100;
            Date exerciseDate = new Date(2017, 9, 30);
            var share = new Share("AAA", TestHelpers.ZAR);
            var option = new EuropeanOption(share, PutOrCall.Call, strike, exerciseDate);

            double spot = 100;
            double rate = 0.05;
            double div = 0.00;
            double vol = 0.20;
            Date valueDate = new Date(2016, 9, 30);

            var price = option.BlackScholesPrice(valueDate, spot, vol, rate, div);
            var delta = option.BlackScholesDelta(valueDate, spot, vol, rate, div);
            var gamma = option.BlackScholesGamma(valueDate, spot, vol, rate, div);
            var vega = option.BlackScholesVega(valueDate, spot, vol, rate, div);
            var theta = option.BlackScholesTheta(valueDate, spot, vol, rate, div);
            var rho = option.BlackScholesRho(valueDate, spot, vol, rate, div);

            Assert.AreEqual(10.4505835721856, price, 1e-8);
            Assert.AreEqual(0.636830651175619, delta, 1e-8);
            Assert.AreEqual(0.0187620173458469, gamma, 1e-8);
            Assert.AreEqual(37.5240346916938, vega, 1e-8);
            Assert.AreEqual(-6.4140275464382, theta, 1e-8);
            Assert.AreEqual(53.2324815453763, rho, 1e-8);
        }

        [TestMethod]
        public void TestEuropeanOpriceImpliedVol()
        {
            
            double strike = 100;
            Date exerciseDate = new Date(2017, 9, 30);
            var share = new Share("AAA", TestHelpers.ZAR);
            var option = new EuropeanOption(share, PutOrCall.Call, strike, exerciseDate);

            double spot = 100;
            double rate = 0.05;
            double div = 0.00;
            double price = 10.4505835721856;
            Date valueDate = new Date(2016, 9, 30);

            var impliedvol = option.BlackScholesImpliedVol(valueDate, spot, rate, div, price);

            Assert.AreEqual(0.20, impliedvol, 1e-4);
        }
    }
}
