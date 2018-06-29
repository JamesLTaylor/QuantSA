using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Products.Rates;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation;
using QuantSA.Valuation.Models.Rates;

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
            var hullWiteSim = new HullWhite1F(Currency.ZAR, a, vol, flatCurveRate, flatCurveRate);

            var coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);
            var callableBond = new CallableBond();
            var value1 = coordinator.Value(new Product[] {callableBond}, valueDate);
        }
    }
}