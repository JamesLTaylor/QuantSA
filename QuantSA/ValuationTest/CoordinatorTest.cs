using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using QuantSA.General;
using QuantSA.Valuation;

namespace ValuationTest
{
    [TestClass]
    public class CoordinatorTest
    {
        /// <summary>
        /// Tests the that if more than one model can simulate an index then an error is thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestIndexOnlyProvidedOnce()
        {
            Date exerciseDate = new Date(2017, 08, 28);
            string shareCode = "AAA";
            double strike = 100.0;
            Product p = new EuropeanOption(new Share(shareCode, Currency.ZAR), strike, exerciseDate);

            Share[] shares = new Share[] { new Share("AAA", Currency.ZAR) };// One needs to know the index that will be required by the product to simulate it.
            Date valueDate = new Date(2016, 08, 28);
            double[] divYield = new double[] { 0.02 };
            double[] vol = new double[] { 0.22 };
            double[] spotPrice = new double[] { 100.0 };
            double[,] correlations = new double[,] { { 1.0 } };
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate,
                new Date[] { valueDate, valueDate.AddMonths(120) },
                new double[] { 0.07, 0.07 });
            IFloatingRateSource[] rateForecastCurves = new IFloatingRateSource[0];

            EquitySimulator sim = new EquitySimulator(shares, spotPrice, vol, divYield,
                correlations, discountCurve, rateForecastCurves);

            Coordinator coordinator = new Coordinator(sim, new List<Simulator>() { sim }, 1000);

            double value = coordinator.Value(new Product[] {p}, valueDate);
        }

        /// <summary>
        /// Tests the that the Coordinator can value the same as Black Scholes
        /// </summary>
        [TestMethod]
        public void TestValuationCoordinator()
        {            
            Date exerciseDate = new Date(2017, 08, 28);
            string shareCode = "AAA";
            double strike = 100.0;
            Product p = new EuropeanOption(new Share(shareCode, Currency.ZAR), strike, exerciseDate);

            Share[] shares = new Share[] { new Share(shareCode, Currency.ZAR) };// One needs to know the index that will be required by the product to simulate it.
            Date valueDate = new Date(2016, 08, 28);
            double[] divYield = new double[] { 0.02 };
            double[] vol = new double[] { 0.22 };
            double[] spotPrice = new double[] { 100.0 };
            double[,] correlations = new double[,] { { 1.0 } };
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate,
                new Date[] { valueDate, valueDate.AddMonths(120) },
                new double[] { 0.07, 0.07 });
            IFloatingRateSource[] rateForecastCurves = new IFloatingRateSource[0];

            EquitySimulator sim = new EquitySimulator(shares, spotPrice, vol, divYield,
                correlations, discountCurve, rateForecastCurves);

            Coordinator coordinator = new Coordinator(sim, new List<Simulator>(), 10000);
            double value = coordinator.Value(new Product[] { p }, valueDate);
            double refValue = Formulae.BlackScholes(PutOrCall.Call, strike, (exerciseDate - valueDate) / 365.0, spotPrice[0],
                                                    vol[0], 0.07, divYield[0]);
            Assert.AreEqual(refValue, value, refValue*0.05);
        }
    }
}
