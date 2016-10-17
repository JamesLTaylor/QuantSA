using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Accord.Statistics.Distributions.Multivariate;
using Accord.Math;
using Accord.Statistics;

namespace GeneralTest
{
    [TestClass]
    public class AccordTest
    {
        /// <summary>
        /// Tests the multivariate normal simulator.  I don't really doubt it but I set this up
        /// to trouble shoot something else and it does not harm to leave it here.
        /// </summary>
        [TestMethod]
        public void TestMultivariateNormalSim()
        {
            double[,] correlations = { { 1.0, 0.4, 0.5},
                                    {0.4, 1.0, 0.6 },
                                    {0.5, 0.6, 1.0 }};
            MultivariateNormalDistribution normal = new MultivariateNormalDistribution(Vector.Zeros(3), correlations);
            int N = 10000;
            double[,] X = new double[N, 3];
            for (int i = 0; i < N; i++)
            {
                double[] dW = normal.Generate();
                X[i, 0] = dW[0];
                X[i, 1] = dW[1];
                X[i, 2] = dW[2];
            }
            double[,] corr = X.Correlation();
            Assert.AreEqual(corr[1, 0], 0.4, 0.05);
            Assert.AreEqual(corr[2, 0], 0.5, 0.05);
            Assert.AreEqual(corr[2, 1], 0.6, 0.05);
        }
    }
}
