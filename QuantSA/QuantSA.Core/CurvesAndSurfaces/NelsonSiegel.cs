using System;
using System.Linq;
using Accord.Math.Optimization;
using QuantSA.Shared.CurvesAndSurfaces;
using QuantSA.Shared.Dates;

namespace QuantSA.Core.CurvesAndSurfaces
{
    /// <summary>
    /// A Nelson Siegel parametric curve.
    /// </summary>
    public class NelsonSiegel : ICurve
    {
        private readonly Date _anchorDate;
        private readonly double _beta0;
        private readonly double _beta1;
        private readonly double _beta2;
        private readonly double _tau;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="anchorDate"></param>
        /// <param name="beta0"></param>
        /// <param name="beta1"></param>
        /// <param name="beta2"></param>
        /// <param name="tau"></param>
        public NelsonSiegel(Date anchorDate, double beta0, double beta1, double beta2, double tau)
        {
            _anchorDate = anchorDate;
            _beta0 = beta0;
            _beta1 = beta1;
            _beta2 = beta2;
            _tau = tau;
        }


        /// <summary>
        ///  Interpolate the rate at required date.        
        /// </summary>
        /// <param name="date">The date at which the rate is required.</param>
        /// <returns></returns>
        public double InterpAtDate(Date date)
        {
            return Interp(_beta0, _beta1, _beta2, _tau, date - _anchorDate);
        }
        
        /// <summary>
        /// Fits a Nelson Siegel curve to data
        /// </summary>
        /// <param name="anchorDate"></param>
        /// <param name="dates"></param>
        /// <param name="rates"></param>
        /// <returns></returns>
        public static NelsonSiegel Fit(Date anchorDate, Date[] dates, double[] rates)
        {
            var times = new double[dates.Length];
            for (var i = 0; i < dates.Length; i++) times[i] = dates[i] - anchorDate;

            Func<double[], double> f = x => ErrorFunction(x, times, rates);

            var nm = new NelderMead(4, f);
            var success = nm.Minimize(new[] {rates[0], rates[0], rates[0], times.Last() / 5.0});
            var minValue = nm.Value;
            var solution = nm.Solution;
            var curve = new NelsonSiegel(anchorDate, solution[0], solution[1], solution[2], solution[3]);

            return curve;
        }

        private static double ErrorFunction(double[] parameters, double[] t, double[] r)
        {
            double error = 0;
            for (var i = 0; i < t.Length; i++)
            {
                var diff = Interp(parameters[0], parameters[1], parameters[2], parameters[3], t[i]) - r[i];
                error += diff * diff;
            }

            return error;
        }


        /// <summary>
        /// Apply the Nelson Siegel formula to a time.
        /// </summary>
        /// <param name="beta0"></param>
        /// <param name="beta1"></param>
        /// <param name="beta2"></param>
        /// <param name="tau"></param>
        /// <param name="t">time at which the rate is required</param>
        /// <returns></returns>
        private static double Interp(double beta0, double beta1, double beta2, double tau, double t)
        {
            var rate = beta0 +
                       (beta1 + beta2) * (1 - Math.Exp(-t / tau)) * tau / t -
                       beta2 * Math.Exp(-t / tau);

            return rate;
        }
    }
}