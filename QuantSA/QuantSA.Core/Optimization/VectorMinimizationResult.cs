using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;

namespace QuantSA.Core.Optimization
{
    /// <summary>
    /// Based on <see cref="MinimizationResult" />.
    /// </summary>
    public class VectorMinimizationResult
    {
        public IObjectiveVectorFunction FunctionInfoAtMinimum { get; set; }
        public int Iterations { get; set; }
        public Vector<double> MinimizingPoint { get; set; }
        public ExitCondition ReasonForExit { get; set; }
    }

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
}