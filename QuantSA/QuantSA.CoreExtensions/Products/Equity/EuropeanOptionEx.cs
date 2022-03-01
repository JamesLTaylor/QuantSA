using System;
using System.Collections.Generic;
using System.Text;
using QuantSA.Core.Products.Equity;
using QuantSA.Shared;
using QuantSA.Shared.Dates;
using MathNet.Numerics.RootFinding;
using QuantSA.Core.Formulae;
using QuantSA.Valuation.Models.Equity;
using QuantSA.Core.RootFinding;
using MathNet.Numerics.Distributions;

namespace QuantSA.CoreExtensions.Products.Equity
{
    public static class EuropeanOptionEx
    {
        public enum OptionPriceandGreeks
        {
            Price,
            Delta,
            Gamma,
            Vega,
            Theta,
            Rho
        }

        private static double D1(double spot, double strike, double vol, double rate, double div, double T)
        {
            double D1 = (Math.Log(spot / strike) + (rate - div + 0.5 * vol * vol)) / (vol * Math.Sqrt(T));
            return D1;
        }

        private static double D2(double spot, double strike, double vol, double rate, double div, double T)
        {
            double D2 = (Math.Log(spot / strike) + (rate - div - 0.5 * vol * vol)) / (vol * Math.Sqrt(T));
            return D2;
        }

        /// <summary>
        /// Gereralized Black, Scholes, Merton formula for European options with dividend
        /// </summary>
        /// <returns>price of the option</returns>
        public static double BlackScholesPrice(this EuropeanOption option, Date valueDate, double spot, double vol, double rate, double div)
        {
            var T = (double)(option._exerciseDate - valueDate) / 365;
            double price;
            price = BlackEtc.BlackScholes(option._putOrCall, option._strike, T, spot, vol, rate, div);

            return price;
        }

        /// <summary>
        /// Delta = first derivative of price with respect to underlying price.
        /// </summary>
        /// <returns>delta of the option</returns>
        public static double BlackScholesDelta(this EuropeanOption option, Date valueDate, double spot, double vol, double rate, double div)
        {
            var T = (double)(option._exerciseDate - valueDate) / 365;
            double d1 = D1(spot, option._strike, vol, rate, div, T);
            double delta;
            double dtq = Math.Exp(-div * T);

            var dist = new Normal();
            if (option._putOrCall == PutOrCall.Call)
            {
                delta =  dtq* dist.CumulativeDistribution(d1);
            }
            else
            {
                delta = dtq * (dist.CumulativeDistribution(d1) - 1);
            }
            return delta;
        }
        
        /// <summary>
        /// Gamma = second derivative of price with respect to underlying price.
        /// </summary>
        /// <returns>gamma of the option</returns>
        public static double BlackScholesGamma(this EuropeanOption option, Date valueDate, double spot, double vol, double rate, double div)
        {
            var dist = new Normal();
            var T = (double)(option._exerciseDate - valueDate) / 365;
            double d1 = D1(spot, option._strike, vol, rate, div, T);
            double gamma;

            gamma = (dist.Density(d1) * Math.Exp(-div * T)) / (spot * vol * Math.Sqrt(T));

            return gamma;
        }

        /// <summary>
        /// Vega = first derivative of price with respect to volatility.
        /// </summary>
        /// <returns>vega of the option</returns>
        public static double BlackScholesVega(this EuropeanOption option, Date valueDate, double spot, double vol, double rate, double div)
        {
            var dist = new Normal();
            var T = (double)(option._exerciseDate - valueDate) / 365;
            double d1 = D1(spot, option._strike, vol, rate, div, T);
            double vega;

            vega = spot * Math.Exp(-div * T) * dist.Density(d1) * Math.Sqrt(T);

            return vega;
        }

        /// <summary>
        /// Theta = first derivative of price with respect to time to expiration.
        /// </summary>
        /// <returns>theta of the option</returns>
        public static double BlackScholesTheta(this EuropeanOption option, Date valueDate, double spot, double vol, double rate, double div)
        {
            var dist = new Normal();
            var T = (double)(option._exerciseDate - valueDate) / 365;
            double d1 = D1(spot, option._strike, vol, rate, div, T);
            double d2 = D2(spot, option._strike, vol, rate, div, T);
            double theta;

            var flag = (double)option._putOrCall;

            double t1 = (Math.Exp(-div * T) * spot * dist.Density(d1) * vol * 0.5) / Math.Sqrt(T);
            double t2 = div * Math.Exp(-div * T) * spot * dist.CumulativeDistribution(flag * d1);
            double t3 = rate * option._strike * Math.Exp(-rate * T) * dist.CumulativeDistribution(flag * d2);

            if (option._putOrCall == PutOrCall.Call)
            {
                theta = -t1 + t2 - t3;
            }
            else
            {
                theta = -t1 - t2 + t3;
            }

            return theta;
        }

        /// <summary>
        /// Rho = first derivative of price with respect to the risk-free rate.
        /// </summary>
        /// <returns>rho of the option</returns>
        public static double BlackScholesRho(this EuropeanOption option, Date valueDate, double spot, double vol, double rate, double div)
        {
            var dist = new Normal();
            var T = (double)(option._exerciseDate - valueDate) / 365;
            double d2 = D2(spot, option._strike, vol, rate, div, T);
            double rho;

            var flag = (double)option._putOrCall;

            rho = flag * T * option._strike * Math.Exp(-rate * T) * dist.CumulativeDistribution(flag * d2);

            return rho;
        }

        public static double BlackScholesPriceAndGreeks(this EuropeanOption option, Date valueDate, double spot, double vol, double rate, double div, OptionPriceandGreeks greek)
        {
            double value = Double.NaN;

            if (greek == OptionPriceandGreeks.Price)
            {
                value = BlackScholesPrice(option, valueDate, spot, vol, rate, div);
            }
            else if (greek == OptionPriceandGreeks.Delta)
            {
                value = BlackScholesDelta(option, valueDate, spot, vol, rate, div);
            }
            else if (greek == OptionPriceandGreeks.Gamma)
            {
                value = BlackScholesGamma(option, valueDate, spot, vol, rate, div);
            }
            else if (greek == OptionPriceandGreeks.Vega)
            {
                value = BlackScholesVega(option, valueDate, spot, vol, rate, div);
            }
            else if (greek == OptionPriceandGreeks.Theta)
            {
                value = BlackScholesTheta(option, valueDate, spot, vol, rate, div);
            }
            else if (greek == OptionPriceandGreeks.Rho)
            {
                value = BlackScholesRho(option, valueDate, spot, vol, rate, div);
            }
            return value;
        }

        public static double BlackScholesImpliedVol(this EuropeanOption option, Date valueDate, double spot, double rate, double div, double price)
        {
            Func<double, double> option_price = x => BlackScholesPrice(option, valueDate, spot, x, rate, div) - price;

            double impliedvol = Brent.FindRoot(option_price, 1e-4, 1000, 1e-8, 1000);

            return impliedvol;
        }

        public static double MonteCarloPrice(double[,] stockpath, double strike, double rate, double T, double flag)
        {
            double sum = 0;

            for (int i = 0; i < stockpath.GetLength(0); i++)
            {
                sum += Math.Max(0, flag * (stockpath[i, 0] - strike));
            }

            double price = (sum / stockpath.GetLength(0)) * Math.Pow(Math.E, -rate * T); ;

            return price;
        }

        public static double MonteCarloPriceAndGreeks(this EuropeanOption option, Date valueDate, double spot, double vol, double rate, double div, OptionPriceandGreeks greek, int numOfSims = 100000)
        {
            var T = (double)(option._exerciseDate - valueDate) / 365;
            var flag = (double)option._putOrCall;

            double bump = 0.01;
            var paths = GBMEquitySimulator.StockPathSimulator(spot, vol, div, rate, T, numOfSims, 1, bump);

            var stockpaths = paths[0];
            var stockpaths_up = paths[1];
            var stockpaths_down = paths[2];

            double value = Double.NaN;

            if (greek == OptionPriceandGreeks.Price)
            {
                value = MonteCarloPrice(stockpaths, option._strike, flag, rate, T);
            }
            else if (greek == OptionPriceandGreeks.Delta)
            {
                value = (MonteCarloPrice(stockpaths_up, option._strike, flag, rate, T) - MonteCarloPrice(stockpaths, option._strike, flag, rate, T))/ bump;
            }
            else if (greek == OptionPriceandGreeks.Gamma)
            {
                value = (MonteCarloPrice(stockpaths_up, option._strike, flag, rate, T) - 2*MonteCarloPrice(stockpaths, option._strike, flag, rate, T) + MonteCarloPrice(stockpaths_down, option._strike, flag, rate, T)) / (bump*bump);
            }
            // To do : create monte carlo vega, theta, and rho
            else if (greek == OptionPriceandGreeks.Vega)
            {
                value = BlackScholesVega(option, valueDate, spot, vol, rate, div);
            }
            else if (greek == OptionPriceandGreeks.Theta)
            {
                value = BlackScholesTheta(option, valueDate, spot, vol, rate, div);
            }
            else if (greek == OptionPriceandGreeks.Rho)
            {
                value = BlackScholesRho(option, valueDate, spot, vol, rate, div);
            }
            return value;
        }
        
    }
}
