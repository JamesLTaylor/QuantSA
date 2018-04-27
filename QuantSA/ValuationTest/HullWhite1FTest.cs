using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Valuation;
using QuantSA.General;
using Accord.Math;
using Accord.Statistics;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.Dates;

namespace ValuationTest
{
    [TestClass]
    public class HullWhite1FTest
    {
        [TestMethod]
        public void TestHullWhite1FForwardsLowRates()
        {
            Date valueDate = new Date(2016, 9, 17);
            double flatRate = 0.01;
            HullWhite1F usdRatesSim = new HullWhite1F(Currency.USD, 0.05, 0.01, flatRate, flatRate, valueDate);
            usdRatesSim.AddForecast(FloatingIndex.LIBOR3M);

            List<Date> simDates = new List<Date>();
            simDates.Add(valueDate.AddMonths(24));
            simDates.Add(simDates[0].AddTenor(FloatingIndex.LIBOR3M.tenor));
            usdRatesSim.Reset();
            usdRatesSim.SetNumeraireDates(simDates);
            usdRatesSim.Prepare();

            int N = 10000;
            double[,] simFwdValues = Matrix.Zeros(N, 2);

            for (int i = 0; i < N; i++)
            {
                usdRatesSim.RunSimulation(i);
                simFwdValues[i, 0] = usdRatesSim.GetIndices(FloatingIndex.LIBOR3M, simDates)[0];
                simFwdValues[i, 1] = 1.0/usdRatesSim.Numeraire(simDates[1]);
            }
            double dt = (simDates[1] - simDates[0]) / 365.0;
            double impliedFwd = (Math.Exp(flatRate * dt) - 1) / dt;
            double[] discountedFRA = simFwdValues.GetColumn(0).Subtract(impliedFwd);
            discountedFRA = Elementwise.Multiply(discountedFRA, simFwdValues.GetColumn(1));
            double actual = discountedFRA.Mean();
            Assert.AreEqual(0.0, actual, 1e-4);
        }
    }
}
