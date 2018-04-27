using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Primitives;
using QuantSA.Primitives.Curves;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.MarketObservables;
using QuantSA.Primitives.Products;
using QuantSA.Primitives.Products.Rates;
using QuantSA.Valuation;
using QuantSA.Valuation.Models.Rates;

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
            Date[] cfDates = { new Date(2016, 12, 23), new Date(2017, 03, 23) };

            FixedLeg legZAR = new FixedLeg(Currency.ZAR, cfDates, new double[] { -16000000, -16000000 }, new double[] { 0.07, 0.07 }, new double[] { 0.25, 0.25 });
            FixedLeg legUSD = new FixedLeg(Currency.USD, cfDates, new double[] { 1000000, 1000000 }, new double[] { 0.01, 0.01 }, new double[] { 0.25, 0.25 });

            // Set up the model
            Date valueDate = new Date(2016, 9, 23);
            Date[] dates = { new Date(2016, 9, 23), new Date(2026, 9, 23) };
            double[] rates = { 0.0725, 0.0725 };
            double[] basisRates = { 0.0735, 0.0735 };
            double[] usdRates = { 0.01, 0.012 };
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate, dates, rates);
            IDiscountingSource zarBasis = new DatesAndRates(Currency.ZAR, valueDate, dates, basisRates);
            IDiscountingSource usdCurve = new DatesAndRates(Currency.USD, valueDate, dates, usdRates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, FloatingIndex.JIBAR3M, dates, rates);
            IFXSource fxSource = new FXForecastCurve(Currency.USD, Currency.ZAR, 13.66, usdCurve, zarBasis);
            DeterminsiticCurves curveSim = new DeterminsiticCurves(discountCurve);
            curveSim.AddRateForecast(forecastCurve);
            curveSim.AddFXForecast(fxSource);
            Coordinator coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);

            // Run the valuation
            double value = coordinator.Value(new Product[] { legZAR, legUSD }, valueDate);
            double refValue = -477027.31; // See GeneralSwapTest.xlsx
            Assert.AreEqual(refValue, value, 0.01);

        }
    }
}
