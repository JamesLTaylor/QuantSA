using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    public enum PutOrCall : int{Call = 1, Put = -1};
    public class Formulae
    {
        /// <summary>
        /// The Black the scholes formula
        /// </summary>
        /// <param name="putOrCall">put or call.</param>
        /// <param name="K">The strike.</param>
        /// <param name="T">The time to maturity in years.</param>
        /// <param name="S">The underlying spot price.</param>
        /// <param name="vol">The lognormal volatility of the underlying price.</param>
        /// <param name="rate">The continuous rate used for drifting the underlying and discounting the payoff.</param>
        /// <param name="div">The contiuous dividend yield that decreased the drift on the underlying.</param>
        /// <returns></returns>
        public static double BlackScholes(PutOrCall putOrCall, double K, double T, double S, double vol, double rate, double div)
        {
            Normal dist = new Normal();
            double sigmaSqrtT = vol * Math.Sqrt(T);
            double d1 = (1 / sigmaSqrtT) * (Math.Log(S / K) + rate - div + 0.5 * vol * vol);
            double d2 = d1 - sigmaSqrtT;
            double F = S * Math.Exp((rate - div) * T);
            return Math.Exp(-rate * T) * (F * dist.CumulativeDistribution(d1) - K * dist.CumulativeDistribution(d2));
        }
    }
}
