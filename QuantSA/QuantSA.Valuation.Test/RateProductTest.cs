using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.MarketData;
using QuantSA.Core.Primitives;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;
using QuantSA.Solution.Test;
using QuantSA.Valuation.Models.Rates;

namespace QuantSA.Valuation.Test
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
            var tenor = Tenor.FromYears(5);
            var swap = TestHelpers.CreateZARSwap(rate, payFixed, notional, startDate, tenor, TestHelpers.Jibar3M);

            // Set up the model
            var valueDate = new Date(2016, 9, 17);
            Date[] dates = {new Date(2016, 9, 17), new Date(2026, 9, 17)};
            double[] rates = {0.07, 0.07};
            IDiscountingSource discountCurve = new DatesAndRates(TestHelpers.ZAR, valueDate, dates, rates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, TestHelpers.Jibar3M, dates, rates);
            var curveSim = new DeterministicCurves(discountCurve);
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
            var tenor = Tenor.FromYears(5);
            var swap = TestHelpers.CreateZARSwap(rate, payFixed, notional, startDate, tenor, TestHelpers.Jibar3M);

            // Set up the model
            var valueDate = new Date(2016, 9, 17);
            var a = 0.05;
            var vol = 0.01;
            var flatCurveRate = 0.07;
            var hullWiteSim = new HullWhite1F(TestHelpers.ZAR, a, vol, flatCurveRate, flatCurveRate);
            hullWiteSim.AddForecast(TestHelpers.Jibar3M);
            var coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 10000);

            // Run the valuation
            var value = coordinator.Value(new IProduct[] {swap}, valueDate);
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
            var tenor = Tenor.FromYears(1);
            var swap = TestHelpers.CreateZARSwap(rate, payFixed, notional, startDate, tenor, TestHelpers.Jibar3M);

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

            var floatLeg = new FloatLeg(TestHelpers.ZAR, paymentDates, new[] {1e6, 1e6, 1e6, 1e6},
                resetDates,
                new[] {TestHelpers.Jibar3M, TestHelpers.Jibar3M, TestHelpers.Jibar3M, TestHelpers.Jibar3M},
                new double[] {0, 0, 0, 0}, accrualFractions);

            // Set up the model
            var valueDate = new Date(2016, 9, 17);
            Date[] dates = {new Date(2016, 9, 17), new Date(2026, 9, 17)};
            double[] rates = {0.07, 0.07};
            IDiscountingSource discountCurve = new DatesAndRates(TestHelpers.ZAR, valueDate, dates, rates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, TestHelpers.Jibar3M, dates, rates);
            var curveSim = new DeterministicCurves(discountCurve);
            curveSim.AddRateForecast(forecastCurve);

            // Run the valuation
            var coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);
            var swapValue = coordinator.Value(new Product[] {swap}, valueDate);
            var floatLegValue = coordinator.Value(new Product[] {floatLeg}, valueDate);

            Assert.AreEqual(swapValue, floatLegValue, 0.01);
        }
    }
}