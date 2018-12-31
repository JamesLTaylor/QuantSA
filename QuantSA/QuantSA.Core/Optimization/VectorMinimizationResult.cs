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
}