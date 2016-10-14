using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.MonteCarlo;
using MonteCarlo;
using QuantSA;
using System.Collections.Generic;

namespace MonteCarloTest
{
    [TestClass]
    public class EquitySimulatorTest
    {
        [TestMethod]
        public void TestEquitySimulatorCall()
        {
            // The model
            Date anchorDate = new Date(2016, 09, 30);
            Share[] shares = new Share[] { new Share("ALSI", Currency.ZAR), new Share("AAA", Currency.ZAR), new Share("BBB", Currency.ZAR) };
            double[] prices = { 200, 50, 100 };
            double[] vols = { 0.22, 0.52, 0.4};
            double[] divYields = { 0.03, 0.0, 0.0 };
            double[,] correlations = { { 1.0, 0.5, 0.5},
                                        {0.5, 1.0, 0.5 },
                                        {0.5, 0.5, 1.0 }};
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, anchorDate,
                new Date[] { anchorDate, anchorDate.AddMonths(36) },
                new double[] { 0.07, 0.07 });
            EquitySimulator sim = new EquitySimulator(shares, prices, vols, divYields, correlations, discountCurve);
            Coordinator coordinator = new Coordinator(sim, new List<Simulator> { }, 10000);

            // Products
            Date exerciseDate = new Date(2017, 08, 28);
            int p;
            double strike;

            // ALSI
            p = 0;
            strike = prices[p] * 1.05;
            Product call0 = new EuropeanOption(shares[p], strike, exerciseDate);
            double value0 = coordinator.Value(new List<Product> { call0 }, anchorDate);
            double refValue0 = Formulae.BlackScholes(PutOrCall.Call, strike, (exerciseDate - anchorDate) / 365.0, prices[p],
                                                    vols[p], 0.07, divYields[p]);
            Assert.AreEqual(refValue0, value0, refValue0 * 0.05);

            // AAA
            p = 1;
            strike = prices[p] * 1.05;
            Product call1 = new EuropeanOption(shares[p], strike, exerciseDate);
            double value1 = coordinator.Value(new List<Product> { call1 }, anchorDate);
            double refValue1 = Formulae.BlackScholes(PutOrCall.Call, strike, (exerciseDate - anchorDate) / 365.0, prices[p],
                                                    vols[p], 0.07, divYields[p]);
            Assert.AreEqual(refValue1, value1, refValue1 * 0.05);

            // BBB
            p = 2;
            strike = prices[p] * 1.05;
            Product call2 = new EuropeanOption(shares[p], strike, exerciseDate);
            double value2 = coordinator.Value(new List<Product> { call2 }, anchorDate);
            double refValue2 = Formulae.BlackScholes(PutOrCall.Call, strike, (exerciseDate - anchorDate) / 365.0, prices[p],
                                                    vols[p], 0.07, divYields[p]);
            Assert.AreEqual(refValue1, value1, refValue1 * 0.05);

            // All at once
            double valueAll = coordinator.Value(new List<Product> { call0, call1, call2 }, anchorDate);
            double refTotal = refValue0 + refValue1 + refValue2;
            Assert.AreEqual(refTotal, valueAll, refTotal * 0.05);
        }
    }
}
