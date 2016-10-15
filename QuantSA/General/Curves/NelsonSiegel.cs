using Accord.Math.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// 
    /// </summary>
    public class NelsonSiegel : ICurve
    {
        private Date anchorDate;
        public double beta0 { get; private set; }
        public double beta1 { get; private set; }
        public double beta2 { get; private set; }
        public double tau { get; private set; }

        private NelsonSiegel(Date anchorDate, double beta0, double beta1, double beta2, double tau)
        {
            this.anchorDate = anchorDate;
            this.beta0 = beta0;
            this.beta1 = beta1;
            this.beta2 = beta2;
            this.tau = tau;
        }

        /// <summary>
        ///  Interpolate the rate at required date.        
        /// </summary>
        /// <param name="date">The date at which the rate is required.</param>
        /// <returns></returns>
        public double InterpAtDate(Date date)
        {
            return Interp(beta0, beta1, beta2, tau, date-anchorDate);            
        }


        #region static methods

        /// <summary>
        /// Fits a Nelson Siegel curve to data
        /// </summary>
        /// <param name="t"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static NelsonSiegel Fit(Date anchorDate, Date[] dates, double[] rates)
        {
            double[] times = new double[dates.Length];
            for (int i=0; i<dates.Length; i++)
            {
                times[i] = dates[i] - anchorDate;
            }

            Func<double[], double> f = (x) => ErrorFunction(x, times, rates);

            var nm = new NelderMead(numberOfVariables: 4, function: f);
            bool success = nm.Minimize(new double[] { rates[0], rates[0], rates[0], times.Last() / 5.0 });
            double minValue = nm.Value;
            double[] solution = nm.Solution;
            NelsonSiegel curve = new NelsonSiegel(anchorDate, solution[0], solution[1], solution[2], solution[3]);

            return curve;
        }

        private static double ErrorFunction(double[] parameters, double[] t, double[] r)
        {
            double error = 0;
            for (int i = 0; i < t.Length; i++)
            {
                double diff = Interp(parameters[0], parameters[1], parameters[2], parameters[3], t[i]) - r[i];
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
        /// <param name="t">time at whihc the rate is required</param>
        /// <returns></returns>
        private static double Interp(double beta0, double beta1, double beta2, double tau, double t)
        {
            double rate = beta0 +
                (beta1 + beta2) * (1 - Math.Exp(-t / tau)) * tau / t -
                beta2 * Math.Exp(-t / tau);
            
            return rate;
        }

        #endregion // static methods
    }
}
