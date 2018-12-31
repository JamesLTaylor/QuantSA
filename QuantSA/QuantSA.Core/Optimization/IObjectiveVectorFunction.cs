using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;

namespace QuantSA.Core.Optimization
{
    /// <summary>
    /// Based on <see cref="IObjectiveFunction" />.
    /// </summary>
    public interface IObjectiveVectorFunction
    {
        Vector<double> Point { get; set; }
        Vector<double> Value { get; set; }
        void EvaluateAt(Vector<double> point);
    }
}