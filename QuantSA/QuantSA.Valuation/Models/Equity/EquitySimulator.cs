using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Statistics.Distributions.Multivariate;
using Newtonsoft.Json;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Exceptions;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Valuation.Models.Equity
{
    /// <summary>
    /// A <see cref="Simulator"/> that can provide realizations of several share prices in a single currency.
    /// </summary>
    public class EquitySimulator : NumeraireSimulator
    {
        private readonly IDiscountingSource _discountCurve;
        private readonly double[] _divYields;
        private readonly double[] _prices;
        private readonly Dictionary<string, IFloatingRateSource> _rateForecastCurves;
        private readonly Share[] _shares;
        private readonly double[] _vols;
        private readonly double[,] _correlations;

        [JsonIgnore] private MultivariateNormalDistribution _normal;
        [JsonIgnore] private Date _anchorDate;
        [JsonIgnore] private Dictionary<int, double[]> _acculatedDivi; 
        [JsonIgnore] private List<Date> _allRequiredDates;
        [JsonIgnore] private Dictionary<int, double[]> _simulation; 


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
            _shares = shares;
            _prices = prices;
            _vols = vols;
            _divYields = divYields;
            _discountCurve = discountCurve;
            _correlations = correlations;
            _rateForecastCurves = new Dictionary<string, IFloatingRateSource>();
            if (rateForecastCurves == null) return;
            foreach (var floatingRateSource in rateForecastCurves)
            {
                if (floatingRateSource == null) continue;
                this._rateForecastCurves.Add(floatingRateSource.GetFloatingIndex().ToString(), floatingRateSource);
            }
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredTimes)
        {
            if (index is FloatRateIndex)
            {
                var result = new double[requiredTimes.Count];
                for (var i = 0; i < requiredTimes.Count; i++)
                    result[i] = _rateForecastCurves[index.ToString()].GetForwardRate(requiredTimes[i]);
                return result;
            }

            if (index is Dividend)
            {
                var result = Vector.Zeros(requiredTimes.Count);
                var shareIndex = _shares.IndexOf(((Dividend) index).Underlying);
                var divCounter = 0;
                foreach (int dateInt in _allRequiredDates)
                {
                    if (dateInt > requiredTimes[divCounter])
                    {
                        divCounter++;
                        if (divCounter >= requiredTimes.Count)
                            break;
                    }

                    result[divCounter] += _acculatedDivi[dateInt][shareIndex];
                }

                return result;
            }
            if (index is Share share)
            {
                var shareIndex = _shares.IndexOf(share);
                var result = new double[requiredTimes.Count];
                for (var i = 0; i < requiredTimes.Count; i++)
                    if (requiredTimes[i] == _anchorDate)
                        result[i] = _prices[shareIndex];
                    else
                        result[i] = _simulation[requiredTimes[i]][shareIndex];
                return result;
            }

            throw new MarketObservableNotSupportedException(
                $"{index} is not supported by this {nameof(EquitySimulator)}");
        }

        /// <summary>
        /// Indicate whether the required share price is simulated by this model
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override bool ProvidesIndex(MarketObservable index)
        {
            if (_shares.Contains(index)) return true;

            if (_rateForecastCurves.ContainsKey(index.ToString())) return true;

            var divIndex = index as Dividend;
            if (divIndex != null)
                return _shares.Contains(divIndex.Underlying);
            return false;
        }

        /// <summary>
        /// Clear the dates that have been set
        /// </summary>
        public override void Reset()
        {
            _allRequiredDates = new List<Date>();
        }


        /// <summary>
        /// Remove duplicate dates and sort the list
        /// </summary>
        public override void Prepare(Date anchorDate)
        {
            _anchorDate = anchorDate;
            _allRequiredDates = _allRequiredDates.Distinct().ToList();
            _allRequiredDates.Sort();
            _normal = new MultivariateNormalDistribution(Vector.Zeros(_prices.Length), _correlations);
        }

        /// <summary>
        /// Run a simulation and store the results for later use by <see cref="GetIndices(MarketObservable, List{Date})"/>
        /// </summary>
        /// <param name="simNumber"></param>
        public override void RunSimulation(int simNumber)
        {
            _simulation = new Dictionary<int, double[]>();
            _acculatedDivi = new Dictionary<int, double[]>();
            var simPrices = _prices.Copy();
            double oldDF = 1;
            double newDF;

            for (var timeCounter = 0; timeCounter < _allRequiredDates.Count; timeCounter++)
            {
                double dt = timeCounter > 0
                    ? _allRequiredDates[timeCounter] - _allRequiredDates[timeCounter - 1]
                    : _allRequiredDates[timeCounter] - _anchorDate.value;
                newDF = _discountCurve.GetDF(_allRequiredDates[timeCounter]);
                var rateDrift = oldDF / newDF;
                oldDF = newDF;
                dt = dt / 365.0;
                var sdt = Math.Sqrt(dt);
                var dW = _normal.Generate();
                _acculatedDivi[_allRequiredDates[timeCounter]] = Vector.Zeros(_shares.Length);
                for (var s = 0; s < _shares.Length; s++)
                {
                    _acculatedDivi[_allRequiredDates[timeCounter]][s] = simPrices[s] * _divYields[s] * dt;
                    simPrices[s] = simPrices[s] * rateDrift *
                                   Math.Exp((-_divYields[s] - 0.5 * _vols[s] * _vols[s]) * dt + _vols[s] * sdt * dW[s]);
                }

                _simulation[_allRequiredDates[timeCounter]] = simPrices.Copy();
            }
        }

        public override void SetRequiredDates(MarketObservable index, List<Date> requiredDates)
        {
            _allRequiredDates.AddRange(requiredDates);
        }

        public override Currency GetNumeraireCurrency()
        {
            return _discountCurve.GetCurrency();
        }

        public override double Numeraire(Date valueDate)
        {
            return 1.0 / _discountCurve.GetDF(valueDate);
        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
            // Nothing needs to be done since we are using a deterministic curve.
        }

        public override double[] GetUnderlyingFactors(Date date)
        {
            var sharePrices = new double[_shares.Length];
            for (var i = 0; i < _shares.Length; i++) sharePrices[i] = GetIndices(_shares[i], new List<Date> {date})[0];
            return sharePrices;
        }
    }
}