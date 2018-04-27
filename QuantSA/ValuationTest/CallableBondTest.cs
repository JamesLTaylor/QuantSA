using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Valuation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Primitives;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.Products;
using QuantSA.Primitives.Products.Rates;
using QuantSA.Valuation.Models.Rates;

namespace ValuationTest
{
    [TestClass]
    public class CallableBondTest
    {
        [TestMethod]
        public void TestCallableBond()
        {
            Date valueDate = new Date(2017, 1, 23);
            double a = 0.05;
            double vol = 0.01;
            double flatCurveRate = 0.18;
            HullWhite1F hullWiteSim = new HullWhite1F(Currency.ZAR, a, vol, flatCurveRate, flatCurveRate, valueDate);

            Coordinator coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);
            CallableBond callableBond = new CallableBond();
            double value1 = coordinator.Value(new Product[] { callableBond }, valueDate);
        }
    }
}
