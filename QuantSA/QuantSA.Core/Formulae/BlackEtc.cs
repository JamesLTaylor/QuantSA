using System;
using MathNet.Numerics.Distributions;

namespace QuantSA.Core.Formulae
{
    public enum PutOrCall
    {
        Call = 1,
        Put = -1
    }

    public class BlackEtc
    {
        /// <summary>
        /// The Black the scholes formula
        /// </summary>
        /// <param name="putOrCall">put or call.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="T">The time to maturity in years.</param>
        /// <param name="spot">The underlying spot price.</param>
        /// <param name="vol">The lognormal volatility of the underlying price.</param>
        /// <param name="rate">The continuous rate used for drifting the underlying and discounting the payoff.</param>
        /// <param name="div">The continuous dividend yield that decreased the drift on the underlying.</param>
        /// <returns></returns>
        public static double BlackScholes(PutOrCall putOrCall, double strike, double T, double spot, double vol, double rate,
            double div)
        {
            var dist = new Normal();
            var sigmaSqrtT = vol * Math.Sqrt(T);
            var d1 = 1 / sigmaSqrtT * (Math.Log(spot / strike) + (rate - div + 0.5 * vol * vol) * T);
            var d2 = d1 - sigmaSqrtT;
            var forward = spot * Math.Exp((rate - div) * T);
            var flag = (double)putOrCall;
            return flag*Math.Exp(-rate * T) * (forward * dist.CumulativeDistribution(flag*d1) - strike * dist.CumulativeDistribution(flag*d2));
        }

        /// <summary>
        /// The Black formula
        /// </summary>
        /// <param name="putOrCall">put or call.</param>
        /// <param name="strike">The strike.</param>
        /// <param name="T">The time to maturity in years from the value date to the exercise date.</param>
        /// <param name="forward">The forward at the option exercise date.</param>
        /// <param name="vol">The lognormal volatility of the underlying price.</param>
        /// <param name="discountFactor">The discount factor from the value date to the settlement date of the option.</param>
        /// <returns></returns>
        public static double Black(PutOrCall putOrCall, double strike, double T, double forward, double vol, double discountFactor)
        {
            var dist = new Normal();
            var sigmaSqrtT = vol * Math.Sqrt(T);
            var d1 = 1 / sigmaSqrtT * (Math.Log(forward / strike) + 0.5 * vol * vol);
            var d2 = d1 - sigmaSqrtT;
            var flag = (double)putOrCall;
            return flag*discountFactor * (forward * dist.CumulativeDistribution(flag * d1) - strike * dist.CumulativeDistribution(flag*d2));
        }
    }
}