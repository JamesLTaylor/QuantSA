using System;
using System.Collections.Generic;
using System.Text;
using QuantSA.Core.Products.Equity;
using QuantSA.Shared;
using QuantSA.Shared.Dates;
using MathNet.Numerics.Distributions;
using QuantSA.Core.Formulae;
using QuantSA.Valuation.Models.Equity;

namespace QuantSA.CoreExtensions.Products.Equity
{
    public static class AsianOptionEx
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

        public static double MonteCarloPrice(double[,] stockpath, double strike, double rate, double T, double flag)
        {
            double sum = 0;
            double total;
            double numSims = stockpath.GetLength(0);
            double timeSteps = stockpath.GetLength(1);
            

            for (int i = 0; i < numSims; i++)
            {
                total = 0;
                for (int j = 0; j < timeSteps; j++)
                {
                    total += stockpath[i, j];
                }

                sum += Math.Max(0, flag*(total / timeSteps - strike));
            }

            double price = (sum / numSims) * Math.Pow(Math.E, -rate * T);

            return price;
        }

        public static double MonteCarloPriceAndGreeks(this AsianOption option, Date valueDate, double spot, double vol, double rate, double div, OptionPriceandGreeks greek, int periods, int numOfSims = 100000)
        {
            var T = (double)(option._exerciseDate - valueDate) / 365;
            var flag = (double)option._putOrCall;

            double bump = 0.01;
            var paths = GBMEquitySimulator.StockPathSimulator(spot, vol, div, rate, T, numOfSims, periods, bump);

            var stockpaths = paths[0];
            var stockpaths_up = paths[1];
            var stockpaths_down = paths[2];

            double value = Double.NaN;

            // To do : create monte carlo vega, theta, and rho
            if (greek == OptionPriceandGreeks.Price)
            {
                value = MonteCarloPrice(stockpaths, option._strike, flag, rate, T);
            }
            else if (greek == OptionPriceandGreeks.Delta)
            {
                value = (MonteCarloPrice(stockpaths_up, option._strike, flag, rate, T) - MonteCarloPrice(stockpaths, option._strike, flag, rate, T)) / bump;
            }
            else if (greek == OptionPriceandGreeks.Gamma)
            {
                value = (MonteCarloPrice(stockpaths_up, option._strike, flag, rate, T) - 2 * MonteCarloPrice(stockpaths, option._strike, flag, rate, T) + MonteCarloPrice(stockpaths_down, option._strike, flag, rate, T)) / (bump * bump);
            }

            return value;
        }
        
    }
}
