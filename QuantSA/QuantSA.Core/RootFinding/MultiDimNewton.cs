using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using QuantSA.Core.Optimization;

namespace QuantSA.Core.RootFinding
{
    public class MultiDimNewton : IVectorRootFinder
    {
        private readonly double _convergenceTolerance;
        private readonly int _maximumIterations;
        private readonly double shift = 1e-8;

        public MultiDimNewton(double convergenceTolerance, int maximumIterations)
        {
            _convergenceTolerance = convergenceTolerance;
            _maximumIterations = maximumIterations;
        }

        public VectorMinimizationResult FindRoot(IObjectiveVectorFunction objective, Vector<double> initialGuess)
        {
            var jacobian = Matrix<double>.Build.Dense(initialGuess.Count, initialGuess.Count);
            var guess = initialGuess.Clone();
            int iterCount;
            for (iterCount = 0; iterCount < _maximumIterations; iterCount++)
            {
                objective.EvaluateAt(guess);
                var baseValues = objective.Value.Clone();
                if (baseValues.AbsoluteMaximum() < _convergenceTolerance) break;
                UpdateJacobian(jacobian, objective);
                var inverse = jacobian.Inverse();
                guess = guess - inverse * baseValues;
            }

            var result = new VectorMinimizationResult
            {
                FunctionInfoAtMinimum = objective,
                Iterations = iterCount,
                MinimizingPoint = guess,
                ReasonForExit = ExitCondition.Converged
            };

            return result;
        }

        /// <summary>
        /// Element (i, j) is the df_i/dx_j. i.e it is the partial derivative of the ith element of f with respect to the jth input.
        /// </summary>
        /// <param name="jacobian"></param>
        /// <param name="objective"></param>
        private void UpdateJacobian(Matrix<double> jacobian, IObjectiveVectorFunction objective)
        {
            var baseValues = objective.Value.Clone();
            var point = objective.Point;
            for (var col = 0; col < jacobian.ColumnCount; col++)
            {
                point[col] += shift;
                objective.EvaluateAt(point);
                var allZero = true;
                for (var row = 0; row < jacobian.RowCount; row++)
                {
                    jacobian[row, col] = (objective.Value[row] - baseValues[row]) / shift;
                    if (allZero && Math.Abs(jacobian[row, col]) > 1e-12) allZero = false;
                }
                if (allZero)
                    throw new ArgumentException($"instrument at position {col} has no sensitivity to any inputs.");
                point[col] -= shift;
            }
        }
    }
}