using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonteCarlo;

namespace QuantSA
{
    [TestClass]
    public class RateProductTest
    {

        [TestMethod]
        public void TestSwap()
        {
            // Make the swap
            double rate = 0.08;
            bool payFixed = true;
            double notional = 1000000;
            Date startDate = new Date(2016, 9, 17);
            Tenor tenor = Tenor.Years(5);
            IRSwap swap = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);

            // Set up the model
            Date valueDate = new Date(2016, 9, 17);
            Date[] dates = { new Date(2016, 9, 17), new Date(2026, 9, 17) };
            double[] rates = { 0.07, 0.07 };
            IDiscountingSource discountCurve = new DatesAndRates(valueDate, dates, rates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, dates, rates);
            DeterminsiticCurves curveSim = new DeterminsiticCurves(Currency.ZAR, discountCurve);
            curveSim.AddForecast(FloatingIndex.JIBAR3M, forecastCurve);
            Coordinator coordinator = new Coordinator(curveSim, new List<Product> { swap }, new List<Simulator>(), 1);

            // Run the valuation
            double value = coordinator.Value(valueDate);
            double refValue = -41838.32; // See RateProductTest.xlsx
            Assert.AreEqual(refValue, value, 0.01);
        }
    }
}
