using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.MarketData;
using QuantSA.Shared.Dates;
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

        }
    }
}
