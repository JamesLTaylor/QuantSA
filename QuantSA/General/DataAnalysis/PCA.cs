using System;
using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics;
using Accord.Statistics.Analysis;

namespace QuantSA.General.DataAnalysis
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

        public static double[,] PCAFromCurves(double[,] curves)
        {
            // Find log returns
            double[,] data = new double[curves.GetLength(0) - 1, curves.GetLength(1)];
            for (int row = 0; row < curves.GetLength(0) - 1; row++)
            {
                for (int col = 0; col < curves.GetLength(1); col++)
                {
                    data[row, col] = Math.Log(curves[row + 1, col] / curves[row, col]);
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
            double[] eigenvalues = singularValues.Pow(2);
            eigenvectors = eigenvectors.Concatenate(eigenvalues);
            //TODO: Use the Accord built in PCA method.

            /*
            double[,] covariance = data.Covariance();
            PrincipalComponentAnalysis pca = PrincipalComponentAnalysis.FromCovarianceMatrix(mean, covariance);

            var components = pca.ComponentVectors;
            double[][] result = components.Concatenate(pca.ComponentProportions);

            double[,] resultRect = new double[result.Length, result[0].Length];
            /*for (int i = 0; i<resultRect.GetLength(0); i++)
            {
                for (int j = 0; j < resultRect.GetLength(1); j++)
                {
                    resultRect[i, j] = result[i][j];
                }
            }
            result.CopyTo(resultRect);
            */

            return eigenvectors;
        }
    }
}
