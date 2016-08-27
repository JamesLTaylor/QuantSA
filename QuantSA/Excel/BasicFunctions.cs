using ExcelDna.Integration;
using MathNet.Numerics.Interpolation;
using System;

namespace Excel
{
    public static class BasicFunctions
    {
        [QuantSAExcelFunction(Description = "A linear interpolator", 
            IsHidden = false,
            Name = "QSA.InterpLinear", 
            Category = "QSA.General",             
            HelpTopic = "https://www.google.co.za")]
        public static object[,] InterpLinear([ExcelArgument(Description = "A vector of x values.  Must be in increasing order")]double[] knownX,
            [ExcelArgument(Description = "A vector of y values.  Must be the same length as knownX")]Double[] knownY, 
            [ExcelArgument(Description = "x values at which interpolation is required.")]Double[,] requiredX)
        {
            LinearSpline spline = LinearSpline.InterpolateSorted(knownX, knownY);
            object[,] result = new object[requiredX.GetLength(0), requiredX.GetLength(1)];

            for (int x = 0; x < requiredX.GetLength(0); x += 1)
            {
                for (int y = 0; y < requiredX.GetLength(1); y += 1)
                {
                    result[x, y] = spline.Interpolate(requiredX[x, y]);
                }
            }
            return result;
        }
    }
}
