using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation;

namespace ValuationTest
{
    /// <summary>
    /// Tests some FX products.
    /// </summary>
    [TestClass]
    public class FXProductTest
    {
        [TestMethod]
        public void TestFixedLegsZARUSD()
        {
            Date[] cfDates = {new Date(2016, 12, 23), new Date(2017, 03, 23)};

            var legZAR = new FixedLeg(Currency.ZAR, cfDates, new double[] {-16000000, -16000000}, new[] {0.07, 0.07},
                new[] {0.25, 0.25});
            var legUSD = new FixedLeg(Currency.USD, cfDates, new double[] {1000000, 1000000}, new[] {0.01, 0.01},
                new[] {0.25, 0.25});

            // Set up the model
            var valueDate = new Date(2016, 9, 23);
            Date[] dates = {new Date(2016, 9, 23), new Date(2026, 9, 23)};
            double[] rates = {0.0725, 0.0725};
            double[] basisRates = {0.0735, 0.0735};
            double[] usdRates = {0.01, 0.012};
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate, dates, rates);
            IDiscountingSource zarBasis = new DatesAndRates(Currency.ZAR, valueDate, dates, basisRates);
            IDiscountingSource usdCurve = new DatesAndRates(Currency.USD, valueDate, dates, usdRates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, FloatRateIndex.JIBAR3M, dates, rates);
            IFXSource fxSource = new FXForecastCurve(Currency.USD, Currency.ZAR, 13.66, usdCurve, zarBasis);
            var curveSim = new DeterminsiticCurves(discountCurve);
            curveSim.AddRateForecast(forecastCurve);
            curveSim.AddFXForecast(fxSource);
            var coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);

            // Run the valuation
            var value = coordinator.Value(new Product[] {legZAR, legUSD}, valueDate);
            var refValue = -477027.31; // See GeneralSwapTest.xlsx
            Assert.AreEqual(refValue, value, 0.01);
        }
    }
}