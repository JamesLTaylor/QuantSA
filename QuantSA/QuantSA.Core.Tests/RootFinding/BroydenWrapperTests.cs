using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Optimization;
using QuantSA.Core.RootFinding;

namespace QuantSA.Core.Tests.RootFinding
{
    [TestClass]
    public class BroydenWrapperTests
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
        public void BroydenWrapper_CanFindRoot()
        {
            var t = new TestFunction();
            var rootfinder = new BroydenWrapper();
            var initialGuess = new DenseVector(new[] { 1.0, 1.0, 1.0 });
            var solution = rootfinder.FindRoot(t, initialGuess);
            t.EvaluateAt(solution.MinimizingPoint);
            Assert.AreEqual(0.0, t.Value.AbsoluteMaximum(), 1e-6);
        }
    }
}
