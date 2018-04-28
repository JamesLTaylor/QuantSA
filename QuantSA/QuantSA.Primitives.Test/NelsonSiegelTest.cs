using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.Primitives.Dates;

namespace GeneralTest
{
    [TestClass]
    public class NelsonSiegelTest
    {
        [TestMethod]
        public void TestNelsonSiegelFitYears()
        {
            var t = new double[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20}.Select(date => new Date(date)).ToArray();
            double[] r =
            {
                0.0745,
                0.0752,
                0.0763,
                0.0776,
                0.0789,
                0.08,
                0.0809,
                0.0817,
                0.0823,
                0.0827,
                0.0833,
                0.0834,
                0.0826
            };
            var fitted = NelsonSiegel.Fit(new Date(0), t, r);

            var fittedVals = Vector<double>.Build.Dense(fitted.InterpAtDates(t));
            var inputVals = Vector<double>.Build.Dense(r);
            var mse = fittedVals.Subtract(inputVals).PointwisePower(2).Mean();
            Assert.AreEqual(0, mse, 5e-7);
        }

        [TestMethod]
        public void TestNelsonSiegelFitDays()
        {
            var t = new double[]
            {
                365,
                730,
                1095,
                1460,
                1825,
                2190,
                2555,
                2920,
                3285,
                3650,
                4380,
                5475,
                7300
            }.Select(date => new Date(date)).ToArray();
            double[] r =
            {
                0.0745,
                0.0752,
                0.0763,
                0.0776,
                0.0789,
                0.08,
                0.0809,
                0.0817,
                0.0823,
                0.0827,
                0.0833,
                0.0834,
                0.0826
            };
            var fitted = NelsonSiegel.Fit(new Date(0), t, r);

            var fittedVals = Vector<double>.Build.Dense(fitted.InterpAtDates(t));
            var inputVals = Vector<double>.Build.Dense(r);
            var mse = fittedVals.Subtract(inputVals).PointwisePower(2).Mean();
            Assert.AreEqual(0, mse, 5e-7);
        }
    }
}