using System;
using MathNet.Numerics.LinearAlgebra.Double;
using QuantSA.Core.Optimization;

namespace QuantSA.Core.RootFinding
{
    /// <summary>
    /// Turn a <see cref="IObjectiveVectorFunction"/> into a <see cref="Func{Array, Array}"/>
    /// </summary>
    internal class FunctionEvaluator
    {
        private readonly IObjectiveVectorFunction _objective;

        internal FunctionEvaluator(IObjectiveVectorFunction objective)
        {
            _objective = objective;
        }

        internal double[] Eval(double[] point)
        {
            _objective.EvaluateAt(new DenseVector(point));
            return _objective.Value.AsArray();
        }
    }
}