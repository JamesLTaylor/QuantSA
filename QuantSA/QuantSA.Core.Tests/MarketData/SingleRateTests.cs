using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.MarketData;
using QuantSA.Core.Serialization;
using QuantSA.Shared.Dates;
using QuantSA.Solution.Test;

namespace QuantSA.Core.Tests.MarketData
{
    [TestClass]
    public class SingleRateTests
    {
        private readonly Date _anchorDate = new Date("2018-12-30");

        [TestMethod]
        public void SingleRate_CanClone()
        {
            var curve = new SingleRate(0.07, _anchorDate, TestHelpers.ZAR);
            var newCurve = (SingleRate) Cloner.Clone(curve);
            var testDate = curve.GetAnchorDate().AddMonths(12);
            Assert.AreEqual(curve.GetDF(testDate), newCurve.GetDF(testDate), 1e-8);
        }
    }
}