using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Products.Rates;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation;

namespace ValuationTest
{
    [TestClass]
    public class CallableBondTest
    {
        [TestMethod]
        public void TestCallableBond()
        {
            var valueDate = new Date(2017, 1, 23);
            var a = 0.05;
            var vol = 0.01;
            var flatCurveRate = 0.18;
            var hullWiteSim = new HullWhite1F(Currency.ZAR, a, vol, flatCurveRate, flatCurveRate, valueDate);

            var coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);
            var callableBond = new CallableBond();
            var value1 = coordinator.Value(new Product[] {callableBond}, valueDate);
        }
    }
}