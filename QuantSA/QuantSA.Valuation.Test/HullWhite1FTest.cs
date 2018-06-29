using System;
using System.Collections.Generic;
using Accord.Math;
using Accord.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation;
using QuantSA.Valuation.Models.Rates;

namespace ValuationTest
{
    [TestClass]
    public class HullWhite1FTest
    {
        [TestMethod]
        public void TestHullWhite1FForwardsLowRates()
        {
            var valueDate = new Date(2016, 9, 17);
            var flatRate = 0.01;
            var usdRatesSim = new HullWhite1F(Currency.USD, 0.05, 0.01, flatRate, flatRate);
            usdRatesSim.AddForecast(FloatRateIndex.LIBOR3M);

            var simDates = new List<Date>();
            simDates.Add(valueDate.AddMonths(24));
            simDates.Add(simDates[0].AddTenor(FloatRateIndex.LIBOR3M.tenor));
            usdRatesSim.Reset();
            usdRatesSim.SetNumeraireDates(simDates);
            usdRatesSim.Prepare(valueDate);

            var N = 10000;
            var simFwdValues = Matrix.Zeros(N, 2);

            for (var i = 0; i < N; i++)
            {
                usdRatesSim.RunSimulation(i);
                simFwdValues[i, 0] = usdRatesSim.GetIndices(FloatRateIndex.LIBOR3M, simDates)[0];
                simFwdValues[i, 1] = 1.0 / usdRatesSim.Numeraire(simDates[1]);
            }

            var dt = (simDates[1] - simDates[0]) / 365.0;
            var impliedFwd = (Math.Exp(flatRate * dt) - 1) / dt;
            var discountedFRA = simFwdValues.GetColumn(0).Subtract(impliedFwd);
            discountedFRA = Elementwise.Multiply(discountedFRA, simFwdValues.GetColumn(1));
            var actual = discountedFRA.Mean();
            Assert.AreEqual(0.0, actual, 1e-4);
        }
    }
}