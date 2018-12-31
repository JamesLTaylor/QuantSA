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
                guess = guess - jacobian.Inverse() * baseValues;
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

        private void UpdateJacobian(Matrix<double> jacobian, IObjectiveVectorFunction objective)
        {
            var baseValues = objective.Value.Clone();
            var point = objective.Point;
            for (var col = 0; col < jacobian.ColumnCount; col++)
            {
                point[col] += shift;
                objective.EvaluateAt(point);
                for (var row = 0; row < jacobian.RowCount; row++)
                    jacobian[row, col] = (objective.Value[row] - baseValues[row]) / shift;

                point[col] -= shift;
            }
        }
    }
}