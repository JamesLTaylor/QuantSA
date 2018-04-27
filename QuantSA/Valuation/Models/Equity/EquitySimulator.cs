using Accord.Statistics.Distributions.Multivariate;
using System;
using System.Collections.Generic;
using Accord.Math;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.General;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.Dates;

namespace QuantSA.Valuation
{
    /// <summary>
    /// A <see cref="Simulator"/> that can provide realizations of several share prices in a single currency.
    /// </summary>
    [Serializable]
    public class EquitySimulator : NumeraireSimulator
    {
        MultivariateNormalDistribution normal;
        private double[] vols;
        private MarketObservable[] shares;
        private double[] divYields;
        private IDiscountingSource discountCurve;
        private Dictionary<MarketObservable, IFloatingRateSource> rateForecastCurves;
        private List<Date> allRequiredDates; // the set of all dates that will be simulated.
        private Dictionary<int, double[]> simulation; // stores the simulated share prices at each required date
        private Dictionary<int, double[]> acculatedDivi; // stores the accumlated dividend on at each required date
        private Date anchorDate;
        private double[] prices;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquitySimulator"/> class.
        /// </summary>
        /// <param name="shares">An array of shares that will be simulated.</param>
        /// <param name="prices">The prices of the supplied shares.</param>
        /// <param name="vols">The annualized volatilites of the supplied shares.</param>
        /// <param name="divYields">The continous dividend yields.</param>
        /// <param name="correlations">The correlation matrix for all the shares.</param>
        /// <param name="discountCurve">The discount curve that will be also be used to determine the drift on the shares.</param>
        /// <param name="rateForecastCurves">Deterministic rate forecast curves, used for example when a 
        /// structure includes a loan whose interest needs to be calculated during the simulation.</param>
        public EquitySimulator(Share[] shares, double[] prices, double[] vols, double[] divYields,
            double[,] correlations, IDiscountingSource discountCurve, IFloatingRateSource[] rateForecastCurves)
        {
            this.shares = shares;
            this.prices = prices;
            this.vols = vols;
            this.divYields = divYields;
            this.discountCurve = discountCurve;
            anchorDate = discountCurve.GetAnchorDate();
            normal = new MultivariateNormalDistribution(Vector.Zeros(prices.Length), correlations);
            this.rateForecastCurves = new Dictionary<MarketObservable, IFloatingRateSource>();
            foreach (IFloatingRateSource floatingRateSource in rateForecastCurves)
            {
                this.rateForecastCurves.Add(floatingRateSource.GetFloatingIndex(), floatingRateSource);
            }
                
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredTimes)
        {
            if (index is FloatingIndex)
            {
                double[] result = new double[requiredTimes.Count];
                for (int i = 0; i < requiredTimes.Count; i++)
                {
                    result[i] = rateForecastCurves[index].GetForwardRate(requiredTimes[i]);
                }
                return result;
            }
            if (index is Dividend)
            {
                double[] result = Vector.Zeros(requiredTimes.Count);
                int shareIndex = shares.IndexOf(((Dividend)index).underlying);
                int divCounter = 0;
                foreach (int dateInt in allRequiredDates)
                {
                    if (dateInt > requiredTimes[divCounter])
                    {
                        divCounter++;
                        if (divCounter >= requiredTimes.Count)
                            break;
                    }
                    result[divCounter] += acculatedDivi[dateInt][shareIndex];
                }
                return result;
            }
            else
            {
                int shareIndex = shares.IndexOf(index);
                double[] result = new double[requiredTimes.Count];
                for (int i = 0; i < requiredTimes.Count; i++)
                {
                    if (requiredTimes[i] == anchorDate)
                        result[i] = prices[shareIndex];
                    else
                        result[i] = simulation[requiredTimes[i]][shareIndex];
                }
                return result;
            }
        }

        /// <summary>
        /// Indicate whether the required share price is simulated by this model
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override bool ProvidesIndex(MarketObservable index)
        {
            if (shares.Contains(index))
                return true;
            else if (rateForecastCurves.ContainsKey(index))
                return true;
            else
            {
                Dividend divIndex = index as Dividend;
                if (divIndex != null)
                    return shares.Contains(divIndex.underlying);
                else
                    return false;
            }
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
            acculatedDivi = new Dictionary<int, double[]>();
            double[] simPrices = prices.Copy();
            double oldDF = 1;
            double newDF;          

            for (int timeCounter = 0; timeCounter < allRequiredDates.Count; timeCounter++)
            {
                double dt = timeCounter > 0 ? allRequiredDates[timeCounter] - allRequiredDates[timeCounter - 1] : allRequiredDates[timeCounter] - anchorDate.value;
                newDF = discountCurve.GetDF(allRequiredDates[timeCounter]);
                double rateDrift = oldDF / newDF;
                oldDF = newDF;
                dt = dt / 365.0;
                double sdt = Math.Sqrt(dt);
                double[] dW = normal.Generate();
                acculatedDivi[allRequiredDates[timeCounter]] = Vector.Zeros(shares.Length);
                for (int s = 0; s < shares.Length; s++)
                {
                    acculatedDivi[allRequiredDates[timeCounter]][s] = simPrices[s] * divYields[s] * dt;
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

        public override double[] GetUnderlyingFactors(Date date)
        {
            double[] sharePrices = new double[shares.Length];
            for (int i = 0; i< shares.Length; i++)
            {
                sharePrices[i] = GetIndices(shares[i], new List<Date> { date })[0];
            }
            return sharePrices;
        }
    }
}