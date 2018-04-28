using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.Primitives.Dates;
using QuantSA.Valuation;

namespace ValuationTest
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
            var rate = 0.08;
            var payFixed = true;
            double notional = 1000000;
            var startDate = new Date(2016, 9, 17);
            var tenor = Tenor.Years(5);
            var swap = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);

            // Set up the model
            var valueDate = new Date(2016, 9, 17);
            Date[] dates = {new Date(2016, 9, 17), new Date(2026, 9, 17)};
            double[] rates = {0.07, 0.07};
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate, dates, rates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, FloatingIndex.JIBAR3M, dates, rates);
            var curveSim = new DeterminsiticCurves(discountCurve);
            curveSim.AddRateForecast(forecastCurve);
            var coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);

            // Run the valuation
            var value = coordinator.Value(new Product[] {swap}, valueDate);
            var refValue = -41838.32; // See RateProductTest.xlsx
            Assert.AreEqual(refValue, value, 0.01);
        }

        [TestMethod]
        public void TestSwapHW()
        {
            // Make the swap
            var rate = 0.08;
            var payFixed = true;
            double notional = 1000000;
            var startDate = new Date(2016, 9, 17);
            var tenor = Tenor.Years(5);
            var swap = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);

            // Set up the model
            var valueDate = new Date(2016, 9, 17);
            var a = 0.05;
            var vol = 0.01;
            var flatCurveRate = 0.07;
            var hullWiteSim = new HullWhite1F(Currency.ZAR, a, vol, flatCurveRate, flatCurveRate, valueDate);
            hullWiteSim.AddForecast(FloatingIndex.JIBAR3M);
            var coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 10000);

            // Run the valuation
            var value = coordinator.Value(new Product[] {swap}, valueDate);
            var refValue = -41838.32; // See RateProductTest.xlsx
            Assert.AreEqual(refValue, value, 4000);
        }

        [TestMethod]
        public void TestFloatLeg()
        {
            // Make the reference swap
            var rate = 0.0;
            var payFixed = true;
            double notional = 1000000;
            var startDate = new Date(2016, 9, 17);
            var tenor = Tenor.Years(1);
            var swap = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);

            // Make a FloatLeg
            var resetDates = new Date[4];
            var paymentDates = new Date[4];
            var accrualFractions = new double[4];
            var runningDate = new Date(2016, 9, 17);
            for (var i = 0; i < 4; i++)
            {
                resetDates[i] = new Date(runningDate);
                paymentDates[i] = resetDates[i].AddMonths(3);
                accrualFractions[i] = (paymentDates[i] - resetDates[i]) / 365.0;
                runningDate = paymentDates[i];
            }

            var floatLeg = new FloatLeg(Currency.ZAR, paymentDates, new[] {1e6, 1e6, 1e6, 1e6},
                resetDates,
                new[] {FloatingIndex.JIBAR3M, FloatingIndex.JIBAR3M, FloatingIndex.JIBAR3M, FloatingIndex.JIBAR3M},
                new double[] {0, 0, 0, 0}, accrualFractions);

            // Set up the model
            var valueDate = new Date(2016, 9, 17);
            Date[] dates = {new Date(2016, 9, 17), new Date(2026, 9, 17)};
            double[] rates = {0.07, 0.07};
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate, dates, rates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, FloatingIndex.JIBAR3M, dates, rates);
            var curveSim = new DeterminsiticCurves(discountCurve);
            curveSim.AddRateForecast(forecastCurve);

            // Run the valuation
            var coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);
            var swapValue = coordinator.Value(new Product[] {swap}, valueDate);
            var floatLegValue = coordinator.Value(new Product[] {floatLeg}, valueDate);

            Assert.AreEqual(swapValue, floatLegValue, 0.01);
        }
    }
}