using Accord.Math.Optimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curves
{
    public class NelsonSiegel
    {
        public double beta0 { get; private set; }
        public double beta1 { get; private set; }
        public double beta2 { get; private set; }
        public double tau { get; private set; }

        public NelsonSiegel(double beta0, double beta1, double beta2, double tau)
        {
            this.beta0 = beta0;
            this.beta1 = beta1;
            this.beta2 = beta2;
            this.tau = tau;
        }

        public static NelsonSiegel Fit(double[] t, double[] r)
        {
            Func<double[], double> f = (x) => ErrorFunction(x, t, r);            

            var nm = new NelderMead(numberOfVariables: 4, function: f);
            bool success = nm.Minimize(new double[] { r[0], r[0], r[0], t.Last() / 5.0 });
            double minValue = nm.Value;
            double[] solution = nm.Solution;
            NelsonSiegel curve = new NelsonSiegel(solution[0], solution[1], solution[2], solution[3]);

            double[] fittedValues = curve.Interp(t);
            return curve;
        }

        private static double ErrorFunction(double[] parameters, double[] t, double[] r)
        {
            double error = 0;
            for (int i=0; i<t.Length; i++)
            {
                double diff = Interp(parameters[0], parameters[1], parameters[2], parameters[3], t[i]) - r[i];
                error += diff * diff;
            }
            return error;
        }

        /// <summary>
        ///  Interpolate rates at provided times.        
        /// </summary>
        /// <param name="t">The times at which the rates are required.</param>
        /// <returns></returns>
        public double[] Interp(double[] t)
        {
            double[] result = new double[t.Length];
            for (int i=0; i<t.Length; i++)
            {
                result[i] = Interp(beta0, beta1, beta2, tau, t[i]);
            }
            return result;
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
        public static double Interp(double beta0, double beta1, double beta2, double tau, double t)
        {
            double rate = beta0 +
                (beta1 + beta2) * (1 - Math.Exp(-t / tau)) * tau / t -
                beta2 * Math.Exp(-t / tau);
            
            return rate;
        }
    }
}
