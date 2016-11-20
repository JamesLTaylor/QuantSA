using System;
using System.Collections.Generic;
using QuantSA.General;

namespace QuantSA.Valuation
{
    /// <summary>
    /// Stores regressors at specified forward dates for all paths.
    /// </summary>
    internal class SimulatedRegressors
    {
        private List<Date> dates;
        private int nSims;
        private double[,,] regressors;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulatedRegressors"/> class.  Uses the provided simulators to
        /// find out how many regressors there will be. 
        /// <para/>
        /// The order of the simulators must stay the same and the 
        /// number of regressors that they produce must stay the same.
        /// </summary>
        /// <param name="dates">The dates as whihc regressors will be stored.</param>
        /// <param name="nSims">The number of  simulations.</param>
        public SimulatedRegressors(List<Date> dates, int nSims, List<Simulator> simulators)
        {
            this.dates = dates;
            this.nSims = nSims;
            // run one simulation to see how many indepedent variables each simulator provides
            int regressorCount = 0;
            foreach (Simulator simulator in simulators)
            {
                simulator.RunSimulation(0);
                regressorCount += simulator.GetUnderlyingFactors(dates[0]).Length;
            }

            regressors = new double[nSims, dates.Count, regressorCount];
        }

        public void Add(int simNumber, int dateNumber, int regressorNumber, double value)
        {
            regressors[simNumber, dateNumber, regressorNumber] = value;
        }

        /// <summary>
        /// Gets up to third order polynomial values of the regressors at the specified date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        internal double[][] GetPolynomialValsRegular(Date date)
        {
            int col = dates.FindIndex(d => d == date);
            double[][] result = new double[regressors.GetLength(0)][];
            for (int row = 0; row < regressors.GetLength(0); row++)
            {
                double[] rowValues = new double[1 + 3 * regressors.GetLength(2)];
                rowValues[0] = 1;
                for (int i = 0; i < regressors.GetLength(2); i++)
                {
                    double x = regressors[row, col, i];
                    rowValues[1 + i * 3] = x;
                    rowValues[1 + i * 3 + 1] = x * x;
                    rowValues[1 + i * 3 + 2] = x * x * x;
                }
                result[row] = rowValues;
            }
            return result;            
        }
    }
}