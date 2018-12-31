using MathNet.Numerics.LinearAlgebra;
using QuantSA.Core.Optimization;

namespace QuantSA.Core.RootFinding
{
    /// <summary>
    /// Find the root of a vector valued function.
    /// </summary>
    public interface IVectorRootFinder
    {
        VectorMinimizationResult FindRoot(IObjectiveVectorFunction objective, Vector<double> initialGuess);
    }
}