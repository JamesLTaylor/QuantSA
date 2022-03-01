using System;
using System.Collections.Generic;
using QuantSA.Core.Formulae;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using Accord.Statistics.Distributions.Multivariate;
using MathNet.Numerics.Distributions;
using Accord.Math;

namespace QuantSA.Valuation.Models.Equity
{
    public class GBMEquitySimulator
    {
        public static List<double[,]> StockPathSimulator(double spot, double vol, double divYield, double rate, double timeToExpiry, int numOfSims, int timeSteps, double bump) 
        {
            double dt = (double) timeToExpiry / timeSteps;
            var sdt = Math.Sqrt(dt);

            // Create an instance of the multivariate normal distribution
            double[,] correlations = { { 1.0 } };
            var normal = new MultivariateNormalDistribution(Vector.Zeros(1), correlations);

            double[,] stockpaths = new double[numOfSims, timeSteps];
            double[,] stockpaths_up = new double[numOfSims, timeSteps];
            double[,] stockpaths_down = new double[numOfSims, timeSteps];

            for (int sim = 0; sim < numOfSims; sim++)
            {
                double S_t = spot;
                double S_t_up = spot + bump;
                double S_t_down = spot - bump;

                for (int t = 0; t < timeSteps; t++)
                {
                    double dW = normal.Generate()[0];
                    S_t = S_t * Math.Pow(Math.E, ((rate - divYield - Math.Pow(vol, 2) / 2)*dt + vol * dW * sdt));
                    stockpaths[sim, t] = S_t;

                    S_t_up = S_t_up * Math.Pow(Math.E, ((rate - divYield - Math.Pow(vol, 2) / 2) * dt + vol * dW * sdt));
                    stockpaths_up[sim, t] = S_t_up;

                    S_t_down = S_t_down * Math.Pow(Math.E, ((rate - divYield - Math.Pow(vol, 2) / 2) * dt + vol * dW * sdt));
                    stockpaths_down[sim, t] = S_t_down;
                }
            }

            var paths = new List<double[,]>();
            paths.Add(stockpaths);
            paths.Add(stockpaths_up);
            paths.Add(stockpaths_down);

            return paths;
        }

    }
}
