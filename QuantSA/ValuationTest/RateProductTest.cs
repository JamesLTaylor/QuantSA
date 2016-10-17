using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
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
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate, dates, rates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, FloatingIndex.JIBAR3M, dates, rates);
            DeterminsiticCurves curveSim = new DeterminsiticCurves(discountCurve);
            curveSim.AddRateForecast(forecastCurve);
            Coordinator coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);

            // Run the valuation
            double value = coordinator.Value(new List<Product> { swap }, valueDate);
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
            Coordinator coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 10000);

            // Run the valuation
            double value = coordinator.Value(new List<Product> { swap }, valueDate);
            double refValue = -41838.32; // See RateProductTest.xlsx
            Assert.AreEqual(refValue, value, 4000);
        }

        [TestMethod]
        public void TestFloatLeg()
        {
            // Make the reference swap
            double rate = 0.0;
            bool payFixed = true;
            double notional = 1000000;
            Date startDate = new Date(2016, 9, 17);
            Tenor tenor = Tenor.Years(1);
            IRSwap swap = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);

            // Make a FloatLeg
            Date[] resetDates = new Date[4];
            Date[] paymentDates = new Date[4];
            double[] accrualFractions = new double[4];
            Date runningDate = new Date(2016, 9, 17);
            for (int i =0; i<4; i++) {
                resetDates[i] = new Date(runningDate);
                paymentDates[i] = resetDates[i].AddMonths(3);
                accrualFractions[i] = (paymentDates[i] - resetDates[i])/365.0;
                runningDate = paymentDates[i];
            }

            FloatLeg floatLeg = new FloatLeg(Currency.ZAR, paymentDates, new double[] { 1e6, 1e6, 1e6, 1e6 },
                resetDates,
                new FloatingIndex[] { FloatingIndex.JIBAR3M, FloatingIndex.JIBAR3M, FloatingIndex.JIBAR3M, FloatingIndex.JIBAR3M },
                new double[] { 0, 0, 0, 0 }, accrualFractions);

            // Set up the model
            Date valueDate = new Date(2016, 9, 17);
            Date[] dates = { new Date(2016, 9, 17), new Date(2026, 9, 17) };
            double[] rates = { 0.07, 0.07 };
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate, dates, rates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, FloatingIndex.JIBAR3M, dates, rates);
            DeterminsiticCurves curveSim = new DeterminsiticCurves(discountCurve);
            curveSim.AddRateForecast(forecastCurve);

            // Run the valuation
            Coordinator coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);
            double swapValue = coordinator.Value(new List<Product> { swap }, valueDate);            
            double floatLegValue = coordinator.Value(new List<Product> { floatLeg}, valueDate);
            
            Assert.AreEqual(swapValue, floatLegValue, 0.01);
        }
    }
}
