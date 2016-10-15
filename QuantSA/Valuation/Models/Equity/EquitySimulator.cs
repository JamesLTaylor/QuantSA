using Accord.Statistics.Distributions.Multivariate;
using System;
using System.Collections.Generic;
using Accord.Math;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.General;

namespace QuantSA.Valuation
{
    /// <summary>
    /// A <see cref="Simulator"/> that can provide realizations of several share prices in a single currency.
    /// </summary>
    public class EquitySimulator : NumeraireSimulator
    {
        MultivariateNormalDistribution normal;
        private double[] vols;
        private MarketObservable[] shares;
        private double[] divYields;
        private IDiscountingSource discountCurve;
        private List<Date> allRequiredDates; // the set of all dates that will be simulated.
        private Dictionary<int, double[]> simulation; // stores the simulation at each required date
        private Date anchorDate;
        private double[] prices;

        public EquitySimulator(Share[] shares, double[] prices, double[] vols, double[] divYields,
            double[,] correlations, IDiscountingSource discountCurve)
        {
            this.shares = shares;
            this.prices = prices;
            this.vols = vols;
            this.divYields = divYields;
            this.discountCurve = discountCurve;
            this.anchorDate = discountCurve.getAnchorDate();
            normal = new MultivariateNormalDistribution(Vector.Zeros(prices.Length), correlations);
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredTimes)
        {
            int shareIndex = shares.IndexOf(index);
            double[] result = new double[requiredTimes.Count];
            for (int i = 0; i < requiredTimes.Count; i++)
            {
                result[i] = simulation[requiredTimes[i]][shareIndex];
            }
            return result;
        }

        /// <summary>
        /// Indicate whether the required share price is simulated by this model
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override bool ProvidesIndex(MarketObservable index)
        {
            return shares.Contains(index);
        }

        /// <summary>
        /// Clear the dates that have been set
        /// </summary>
        public override void Reset()
        {
            allRequiredDates = new List<Date>();
        }


        /// <summary>
        /// Remove duplicate dates and sort the list
        /// </summary>
        public override void Prepare()
        {
            allRequiredDates = allRequiredDates.Distinct().ToList();
            allRequiredDates.Sort();
        }

        /// <summary>
        /// Run a simulation and store the results for later use by <see cref="GetIndices(MarketObservable, List{Date})"/>
        /// </summary>
        /// <param name="simNumber"></param>
        public override void RunSimulation(int simNumber)
        {
            simulation = new Dictionary<int, double[]>();
            double[] simPrices = prices.Copy();
            double oldDF = 1;
            double newDF;          

            for (int timeCounter = 0; timeCounter < allRequiredDates.Count; timeCounter++)
            {
                double dt = timeCounter > 0 ? allRequiredDates[timeCounter] - allRequiredDates[timeCounter - 1] : allRequiredDates[timeCounter] - anchorDate.value;
                newDF = discountCurve.GetDF(allRequiredDates[timeCounter]);
                double rateDrift = oldDF / newDF;
                newDF = oldDF;
                dt = dt / 365.0;
                double sdt = Math.Sqrt(dt);
                double[] dW = normal.Generate();
                for (int s = 0; s < shares.Length; s++)
                {
                    simPrices[s] = simPrices[s] * rateDrift *
                        Math.Exp((-divYields[s] - 0.5 * vols[s] * vols[s]) * dt + vols[s] * sdt * dW[s]);
                }
                simulation[allRequiredDates[timeCounter]] = simPrices.Copy();
            }
        }

        public override void SetRequiredDates(MarketObservable index, List<Date> requiredDates)
        {
            allRequiredDates.AddRange(requiredDates);
        }

        public override Currency GetNumeraireCurrency()
        {
            return discountCurve.GetCurrency();
        }

        public override double Numeraire(Date valueDate)
        {
            return 1.0 / discountCurve.GetDF(valueDate);
        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
            // Nothing needs to be done since we are using a deterministic curve.
        }

    }
}