using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.MarketData;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Exceptions;
using QuantSA.Shared.MarketData;
using QuantSA.Solution.Test;

namespace QuantSA.Core.Tests.MarketData
{
    [TestClass]
    public class MarketDataContainerTests
    {
        private readonly Date _anchorDate = new Date("2018-12-30");

        [TestMethod]
        public void MarketDataContainer_CanAddAndGetCurve()
        {
            var curve = new SingleRate(0.07, _anchorDate, TestHelpers.ZAR);
            var mdc = new MarketDataContainer();
            mdc.Set(curve);

            var curveOut = mdc.Get(new DiscountingSourceDescription(TestHelpers.ZAR));
            Assert.IsNotNull(curveOut);
        }

        [TestMethod]
        public void MarketDataContainer_ThrowsOnMissingCurve()
        {
            var curve = new SingleRate(0.07, _anchorDate, TestHelpers.ZAR);
            var mdc = new MarketDataContainer();
            mdc.Set(curve);

            Assert.ThrowsException<MissingMarketDataException>(() => mdc.Get(new DiscountingSourceDescription(TestHelpers.USD)));
        }
    }
}
