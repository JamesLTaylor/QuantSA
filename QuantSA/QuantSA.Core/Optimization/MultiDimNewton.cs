using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace QuantSA.Core.Optimization
{
    public class MultiDimNewton
    {
        private readonly double _convergenceTolerance;
        private readonly int _maximumIterations;
        private readonly double shift = 1e-8;

        public MultiDimNewton(double convergenceTolerance, int maximumIterations)
        {
            _convergenceTolerance = convergenceTolerance;
            _maximumIterations = maximumIterations;
        }

        public VectorMinimizationResult FindMinimum(IObjectiveVectorFunction objective, Vector<double> initialGuess)
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
                guess = guess - baseValues * jacobian.Inverse();
            }

            var result = new VectorMinimizationResult();
            return result;
        }

        private void UpdateJacobian(Matrix<double> jacobian, IObjectiveVectorFunction objective)
        {
            var baseValues = objective.Value.Clone();
            var point = objective.Point;
            for (var row = 0; row < jacobian.RowCount; row++)
            {
                point[row] += shift;
                objective.EvaluateAt(point);
                for (var col = 0; col < jacobian.ColumnCount; col++)
                    jacobian[row, col] = (objective.Value[col] - baseValues[col]) / shift;

                point[row] -= shift;
            }
        }
    }
}
