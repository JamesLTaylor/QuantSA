using System;
using MathNet.Numerics.Interpolation;
using QuantSA.General;
using QuantSA.Primitives.Dates;

namespace QuantSA.Core.CurvesAndSurfaces
{
    public class InterpolatedCurve : ICurve
    {
        private readonly LinearSpline _spline;
        private readonly double[] _xVals;
        private readonly double[] _yVals;

        public InterpolatedCurve(double[] xVals, double[] yVals)
        {
            _xVals = xVals;
            _yVals = yVals;
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
    }
}