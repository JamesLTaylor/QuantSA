using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Formulae;
using QuantSA.Core.Primitives;
using QuantSA.Core.Products.Equity;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Solution.Test;
using QuantSA.Valuation.Models.Equity;
using QuantSA.Valuation.Models.Rates;

namespace QuantSA.Valuation.Test
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
            var exerciseDate = new Date(2017, 08, 28);
            var shareCode = "AAA";
            var strike = 100.0;
            Product p = new EuropeanOption(new Share(shareCode, TestHelpers.ZAR), PutOrCall.Call, strike, exerciseDate);

            var shares = new[]
            {
                new Share("AAA", TestHelpers.ZAR)
            }; // One needs to know the index that will be required by the product to simulate it.
            var valueDate = new Date(2016, 08, 28);
            var divYield = new[] {0.02};
            var vol = new[] {0.22};
            var spotPrice = new[] {100.0};
            var correlations = new[,] {{1.0}};
            IDiscountingSource discountCurve = new DatesAndRates(TestHelpers.ZAR, valueDate,
                new[] {valueDate, valueDate.AddMonths(120)},
                new[] {0.07, 0.07});
            var rateForecastCurves = new IFloatingRateSource[0];

            var sim = new EquitySimulator(shares, spotPrice, vol, divYield,
                correlations, discountCurve, rateForecastCurves);

            var coordinator = new Coordinator(sim, new List<Simulator> {sim}, 1000);

            var value = coordinator.Value(new[] {p}, valueDate);
        }

        /// <summary>
        /// Tests the that the Coordinator can value the same as Black Scholes
        /// </summary>
        [TestMethod]
        public void TestValuationCoordinator()
        {
            var exerciseDate = new Date(2017, 08, 28);
            var shareCode = "AAA";
            var strike = 100.0;
            Product p = new EuropeanOption(new Share(shareCode, TestHelpers.ZAR), PutOrCall.Call, strike, exerciseDate);

            var shares = new[]
            {
                new Share(shareCode, TestHelpers.ZAR)
            }; // One needs to know the index that will be required by the product to simulate it.
            var valueDate = new Date(2016, 08, 28);
            var divYield = new[] {0.02};
            var vol = new[] {0.22};
            var spotPrice = new[] {100.0};
            var correlations = new[,] {{1.0}};
            IDiscountingSource discountCurve = new DatesAndRates(TestHelpers.ZAR, valueDate,
                new[] {valueDate, valueDate.AddMonths(120)},
                new[] {0.07, 0.07});
            var rateForecastCurves = new IFloatingRateSource[0];

            var sim = new EquitySimulator(shares, spotPrice, vol, divYield,
                correlations, discountCurve, rateForecastCurves);

            var coordinator = new Coordinator(sim, new List<Simulator>(), 10000);
            var value = coordinator.Value(new[] {p}, valueDate);
            var refValue = BlackEtc.BlackScholes(PutOrCall.Call, strike, (exerciseDate - valueDate) / 365.0,
                spotPrice[0],
                vol[0], 0.07, divYield[0]);
            Assert.AreEqual(refValue, value, refValue * 0.05);
        }

        [TestMethod]
        public void TestCoordinatorAllData()
        {
            // Make the swap
            var rate = 0.07;
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
            var coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);

            var date = valueDate;
            var endDate = valueDate.AddTenor(tenor);
            var fwdValueDates = new List<Date>();
            while (date < endDate)
            {
                fwdValueDates.Add(date);
                date = date.AddTenor(Tenor.FromDays(10));
            }

            var allDetails = coordinator.GetValuePaths(new Product[] {swap}, valueDate, fwdValueDates.ToArray());
            allDetails.GetNames();
        }
    }
}