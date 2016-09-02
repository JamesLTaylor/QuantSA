using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;

namespace CurvesTest
{
    [TestClass]
    public class NelsonSiegelTest
    {
        [TestMethod]
        public void TestFitYears()
        {
            Double[] t = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20 };
            Double[] r = {0.0745,
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
                            0.0826 };
            NelsonSiegel fitted = NelsonSiegel.Fit(t, r);

            Vector<double> fittedVals = Vector<double>.Build.Dense(fitted.InterpAtTime(t));
            Vector<double> inputVals = Vector<double>.Build.Dense(r);
            double mse = fittedVals.Subtract(inputVals).PointwisePower(2).Mean();
            Assert.AreEqual(0, mse, 5e-7);

        }

        [TestMethod]
        public void TestFitDays()
        {
            Double[] t = {365,
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
                            7300};
            Double[] r = {0.0745,
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
                            0.0826 };
            NelsonSiegel fitted = NelsonSiegel.Fit(t, r);

            Vector<double> fittedVals = Vector<double>.Build.Dense(fitted.InterpAtTime(t));
            Vector<double> inputVals = Vector<double>.Build.Dense(r);
            double mse = fittedVals.Subtract(inputVals).PointwisePower(2).Mean();
            Assert.AreEqual(0, mse, 5e-7);
        }

    }
}
