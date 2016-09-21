using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonteCarlo;

namespace QuantSA
{
    [TestClass]
    public class RateProductTest
    {
        [TestInitialize]
        public void Init()
        {

        }

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
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, FloatingIndex.JIBAR3M, dates, rates);
            DeterminsiticCurves curveSim = new DeterminsiticCurves(Currency.ZAR, discountCurve);
            curveSim.AddForecast(forecastCurve);
            Coordinator coordinator = new Coordinator(curveSim, new List<Product> { swap }, new List<Simulator>(), 1);

            // Run the valuation
            double value = coordinator.Value(valueDate);
            double refValue = -41838.32; // See RateProductTest.xlsx
            Assert.AreEqual(refValue, value, 0.01);
        }

        [TestMethod]
        public void TestSwapHW()
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
            double a = 0.05;
            double vol = 0.01;
            double flatCurveRate = 0.07;
            HullWhite1F hullWiteSim = new HullWhite1F(a, vol, flatCurveRate, flatCurveRate, valueDate);
            hullWiteSim.AddForecast(FloatingIndex.JIBAR3M);
            Coordinator coordinator = new Coordinator(hullWiteSim, new List<Product> { swap }, new List<Simulator>(), 10000);

            // Run the valuation
            double value = coordinator.Value(valueDate);
            double refValue = -41838.32; // See RateProductTest.xlsx
            Assert.AreEqual(refValue, value, 4000);
        }
    }
}
