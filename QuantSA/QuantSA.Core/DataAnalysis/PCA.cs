using System;
using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics;
using Accord.Statistics.Analysis;

namespace QuantSA.General
{
    public class PCA
    {
        public static double[,] CovarianceFromCurves(double[,] curves)
        {
            // Find log returns
            double[,] newData = new double[curves.GetLength(0) - 1, curves.GetLength(1)];
            for (int row = 0; row < curves.GetLength(0) - 1; row++)
            {
                for (int col=0; col<curves.GetLength(1); col++)
                {
                    newData[row, col] = Math.Log(curves[row + 1, col] / curves[row, col]);
                }
            }

            double[,] result = newData.Covariance();
            return result;
        }

        public static double[,] PCAFromCurves(double[,] curves, bool useReturns)
        {
            //TODO: Rather return a ResultStore with eigen vectors and eigen values separate
            // Find log returns
            double[,] data = new double[curves.GetLength(0) - 1, curves.GetLength(1)];
            for (int row = 0; row < curves.GetLength(0) - 1; row++)
            {
                for (int col = 0; col < curves.GetLength(1); col++)
                {
                    if (useReturns)
                        data[row, col] = Math.Log(curves[row + 1, col] / curves[row, col]);
                    else
                        data[row, col] = curves[row + 1, col] - curves[row, col];
                }
            }

            double[] mean = data.Mean(0);
            for (int row = 0; row < data.GetLength(0); row++) {
                for (int col = 0; col < data.GetLength(1); col++) {
                    data[row, col] = data[row, col] - mean[col];
                }
            }


            var svd = new SingularValueDecomposition(data);
            double[] singularValues = svd.Diagonal;
            double[,] eigenvectors = svd.RightSingularVectors;
            double[] eigenvalues = singularValues.Pow(2).Divide(curves.GetLength(0) - 2);

            eigenvectors = eigenvectors.Concatenate(eigenvalues);
            //TODO: Use the Accord built-in PCA method.

            return eigenvectors;
        }
    }
}
