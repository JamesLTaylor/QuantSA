using System;
using MathNet.Numerics.Interpolation;
using QuantSA.Shared.CurvesAndSurfaces;
using QuantSA.Shared.Dates;

namespace QuantSA.Core.CurvesAndSurfaces
{
    public class InterpolatedCurve : ICurve
    {
        private readonly LinearSpline _spline;

        public InterpolatedCurve(double[] xVals, double[] yVals)
        {
            _spline = LinearSpline.InterpolateSorted(xVals, yVals);
        }

        public double InterpAtDate(Date date)
        {
            throw new NotImplementedException();
        }

        public double Interp(double requiredX)
        {
            return _spline.Interpolate(requiredX);
        }

        public double[,] Interp(double[,] requiredX)
        {
            var result = new double[requiredX.GetLength(0), requiredX.GetLength(1)];
            for (var i = 0; i < requiredX.GetLength(0); i++)
            for (var j = 0; j < requiredX.GetLength(1); j++)
                result[i, j] = _spline.Interpolate(requiredX[i, j]);
            return result;
        }
    }
}