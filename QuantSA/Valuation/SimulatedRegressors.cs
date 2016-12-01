using System;
using System.Collections.Generic;
using QuantSA.General;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Statistics.Distributions.Univariate;

namespace QuantSA.Valuation
{
    /// <summary>
    /// Stores regressors at specified forward dates for all paths.
    /// </summary>
    internal class SimulatedRegressors
    {
        private List<Date> dates;
        private int nSims;
        /// <summary>
        /// The regressors.  Index order is simulation number, date number, regressor number
        /// </summary>
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

        /// <summary>
        /// Adds a regressor at a specfied location.
        /// </summary>
        /// <param name="simNumber">The simulation number.</param>
        /// <param name="dateNumber">The forward date number.</param>
        /// <param name="regressorNumber">The regressor number.</param>
        /// <param name="value">The value to be inserted.</param>
        public void Add(int simNumber, int dateNumber, int regressorNumber, double value)
        {
            regressors[simNumber, dateNumber, regressorNumber] = value;
        }

        /// <summary>
        /// Gets the powers of the regressors up to <paramref name="order"/> at the specified date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="order">The maximum power that is computed.</param>
        /// <returns></returns>
        private double[][] GetPolynomialValsRegular(Date date, int order)
        {
            int col = dates.FindIndex(d => d == date);
            double[][] result = new double[regressors.GetLength(0)][];
            for (int row = 0; row < regressors.GetLength(0); row++)
            {
                double[] rowValues = new double[1 + order * regressors.GetLength(2)];
                rowValues[0] = 1;
                for (int i = 0; i < regressors.GetLength(2); i++)
                {
                    double x = regressors[row, col, i];
                    rowValues[1 + i*order] = x;
                    for (int orderCounter = 1; orderCounter < order; orderCounter++)
                    {
                        rowValues[1 + i * order + orderCounter] = rowValues[i * order + orderCounter] * x;                        
                    }
                }
                result[row] = rowValues;
            }
            return result;            
        }

        /// <summary>
        /// Gets a fitted approximation to the forward values.
        /// </summary>
        /// <param name="date">The date at which the regressors should be observed.</param>
        /// <param name="cfs">The sum of the pv of all the cashlows on the path that take place after <paramref name="date"/>.</param>
        /// <returns></returns>
        public double[] FitCFs(Date date, double[] cfs)
        {
            //double[][] inputs = GetPolynomialValsRegular(date, 3);
            double[][] inputs = GetIntrinsic(date, 5);

            var ols = new OrdinaryLeastSquares()
            { UseIntercept = true, IsRobust = true };            
            MultipleLinearRegression regression = ols.Learn(inputs, cfs);
            double[] result = regression.Transform(inputs);
            return result;            
        }

        /// <summary>
        /// Extraxcts all the x for a given date and regressor nuymber.
        /// </summary>
        /// <param name="dateCol">The date column.</param>
        /// <param name="regressorNumber">The regressor number.</param>
        /// <returns></returns>
        private double[] GetSingleX(int dateCol, int regressorNumber)
        {
            double[] result = new double[regressors.GetLength(0)];
            for (int i = 0; i < result.Length; i++)
                result[i] = regressors[i, dateCol, regressorNumber];
            return result;
        }

        /// <summary>
        /// Fits the cashflows to intrinsic functions of x.  i.e. (x-K)^+ and (K-x)^+
        /// </summary>
        /// <returns></returns>
        private double[][] GetIntrinsic(Date date, int order)
        {
            int col = dates.FindIndex(d => d == date);
            double[][] result = new double[regressors.GetLength(0)][];
            double[] xVec = GetSingleX(col, 0);
            EmpiricalDistribution xDist = new EmpiricalDistribution(xVec);
            double[] strikes = new double[order];
            for (int i = 1; i <= order; i++)
            {
                strikes[i-1] = xDist.InverseDistributionFunction((double)i/(order+1));
            }

            for (int row = 0; row < regressors.GetLength(0); row++)
            {
                double[] rowValues = new double[1 + 2*order * regressors.GetLength(2)];
                rowValues[0] = 1;
                for (int i = 0; i < regressors.GetLength(2); i++)
                {
                    double x = regressors[row, col, i];                    
                    for (int orderCounter = 0; orderCounter < order; orderCounter++)
                    {
                        rowValues[1 + i * order + 2 * orderCounter] = Math.Max(0, x - strikes[orderCounter]);
                        rowValues[2 + i * order + 2 * orderCounter] = Math.Max(0, strikes[orderCounter] - x);
                    }
                }
                result[row] = rowValues;
            }
            return result;
        }
    }
}