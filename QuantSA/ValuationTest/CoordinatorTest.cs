using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using QuantSA.Primitives;
using QuantSA.Primitives.Curves;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.Formulae;
using QuantSA.Primitives.MarketObservables;
using QuantSA.Primitives.Products;
using QuantSA.Primitives.Products.Equity;
using QuantSA.Primitives.Products.Rates;
using QuantSA.Valuation;
using QuantSA.Valuation.Models.Equity;
using QuantSA.Valuation.Models.Rates;

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

            double value = coordinator.Value(new Product[] { p }, valueDate);
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
            double refValue = BlackEtc.BlackScholes(PutOrCall.Call, strike, (exerciseDate - valueDate) / 365.0, spotPrice[0],
                                                    vol[0], 0.07, divYield[0]);
            Assert.AreEqual(refValue, value, refValue * 0.05);
        }

        [TestMethod]
        public void TestCoordinatorAllData()
        {
            // Make the swap
            double rate = 0.07;
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
            HullWhite1F hullWiteSim = new HullWhite1F(Currency.ZAR, a, vol, flatCurveRate, flatCurveRate, valueDate);
            hullWiteSim.AddForecast(FloatingIndex.JIBAR3M);
            Coordinator coordinator = new Coordinator(hullWiteSim, new List<Simulator>(), 5000);

            Date date = valueDate;
            Date endDate = valueDate.AddTenor(tenor);
            List<Date> fwdValueDates = new List<Date>();
            while (date < endDate)
            {
                fwdValueDates.Add(date);
                date = date.AddTenor(Tenor.Days(10));
            }
            ResultStore allDetails = coordinator.GetValuePaths(new Product[] { swap }, valueDate, fwdValueDates.ToArray());
            allDetails.GetNames();            
        }
    }
}
