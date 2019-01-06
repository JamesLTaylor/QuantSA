using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Optimization;
using QuantSA.Core.RootFinding;

namespace QuantSA.Core.Tests.RootFinding
{
    [TestClass]
    public class MultiDimNewtonTests
    {
        public class TestFunction : IObjectiveVectorFunction
        {
            public Vector<double> Point { get; set; }
            public Vector<double> Value { get; set; }

            public void EvaluateAt(Vector<double> point)
            {
                Point = point;
                Value = Vector<double>.Build.Dense(3);
                Value[0] = Math.Pow(point[0] - 1.0, 2);
                Value[1] = Math.Pow(point[1] - 2.0, 2);
                Value[2] = Math.Pow(point[2] - 3.0, 2);
            }
        }

        [TestMethod]
        public void MultiDimNewton_CanFindRoot()
        {
            var t = new TestFunction();
            var mdnSolver = new MultiDimNewton(1e-6, 100);
            var initialGuess = new DenseVector(new[] { 1.0, 1.0, 1.0 });
            var solution = mdnSolver.FindRoot(t, initialGuess);
            t.EvaluateAt(solution.MinimizingPoint);
            Assert.AreEqual(0.0, t.Value.AbsoluteMaximum(), 1e-6);
        }
    }
}
