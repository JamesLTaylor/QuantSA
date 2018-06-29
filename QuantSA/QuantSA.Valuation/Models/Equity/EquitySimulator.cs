using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Statistics.Distributions.Multivariate;
using Newtonsoft.Json;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Valuation
{
    /// <summary>
    /// A <see cref="Simulator"/> that can provide realizations of several share prices in a single currency.
    /// </summary>
    public class EquitySimulator : NumeraireSimulator
    {
        private readonly Date anchorDate;
        private readonly IDiscountingSource discountCurve;
        private readonly double[] divYields;
        private readonly MultivariateNormalDistribution normal;
        private readonly double[] prices;
        private readonly Dictionary<MarketObservable, IFloatingRateSource> rateForecastCurves;
        private readonly MarketObservable[] shares;
        private readonly double[] vols;
        private Dictionary<int, double[]> acculatedDivi; // stores the accumulated dividend on at each required date
        private List<Date> allRequiredDates; // the set of all dates that will be simulated.
        private Dictionary<int, double[]> simulation; // stores the simulated share prices at each required date
        [JsonIgnore] private Date _anchorDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquitySimulator"/> class.
        /// </summary>
        /// <param name="shares">An array of shares that will be simulated.</param>
        /// <param name="prices">The prices of the supplied shares.</param>
        /// <param name="vols">The annualized volatilities of the supplied shares.</param>
        /// <param name="divYields">The continuous dividend yields.</param>
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
            if (rateForecastCurves == null) return;
            foreach (var floatingRateSource in rateForecastCurves)
            {
                if (floatingRateSource == null) continue;
                this.rateForecastCurves.Add(floatingRateSource.GetFloatingIndex(), floatingRateSource);
            }
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredTimes)
        {
            if (index is FloatRateIndex)
            {
                var result = new double[requiredTimes.Count];
                for (var i = 0; i < requiredTimes.Count; i++)
                    result[i] = rateForecastCurves[index].GetForwardRate(requiredTimes[i]);
                return result;
            }

            if (index is Dividend)
            {
                var result = Vector.Zeros(requiredTimes.Count);
                var shareIndex = shares.IndexOf(((Dividend) index).underlying);
                var divCounter = 0;
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
                var shareIndex = shares.IndexOf(index);
                var result = new double[requiredTimes.Count];
                for (var i = 0; i < requiredTimes.Count; i++)
                    if (requiredTimes[i] == anchorDate)
                        result[i] = prices[shareIndex];
                    else
                        result[i] = simulation[requiredTimes[i]][shareIndex];
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
            if (shares.Contains(index)) return true;

            if (rateForecastCurves.ContainsKey(index)) return true;

            var divIndex = index as Dividend;
            if (divIndex != null)
                return shares.Contains(divIndex.underlying);
            return false;
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
        public override void Prepare(Date anchorDate)
        {
            _anchorDate = anchorDate;
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
            var simPrices = prices.Copy();
            double oldDF = 1;
            double newDF;

            for (var timeCounter = 0; timeCounter < allRequiredDates.Count; timeCounter++)
            {
                double dt = timeCounter > 0
                    ? allRequiredDates[timeCounter] - allRequiredDates[timeCounter - 1]
                    : allRequiredDates[timeCounter] - anchorDate.value;
                newDF = discountCurve.GetDF(allRequiredDates[timeCounter]);
                var rateDrift = oldDF / newDF;
                oldDF = newDF;
                dt = dt / 365.0;
                var sdt = Math.Sqrt(dt);
                var dW = normal.Generate();
                acculatedDivi[allRequiredDates[timeCounter]] = Vector.Zeros(shares.Length);
                for (var s = 0; s < shares.Length; s++)
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
            var sharePrices = new double[shares.Length];
            for (var i = 0; i < shares.Length; i++) sharePrices[i] = GetIndices(shares[i], new List<Date> {date})[0];
            return sharePrices;
        }
    }
}