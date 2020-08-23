using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Statistics.Distributions.Multivariate;
using Newtonsoft.Json;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation.Models.Rates;

namespace QuantSA.Valuation.Models.RatesFX
{
    public class MultiHWAndFXToy : NumeraireSimulator
    {
        private readonly Dictionary<string, HullWhite1F> _ccySimMap;
        private readonly double[,] _correlations;
        private readonly CurrencyPair[] _currencyPairs;
        private readonly Currency _numeraireCcy;
        private readonly double[] _spots;
        private readonly double[] _vols;
        [JsonIgnore] private MultivariateNormalDistribution _normal;
        [JsonIgnore] private Date _anchorDate;
        [JsonIgnore] private List<Date> _allRequiredDates;
        [JsonIgnore] private Dictionary<int, double[]> _simulation;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiHWAndFXToy"/> class.
        /// </summary>
        /// <param name="anchorDate">The anchor date.</param>
        /// <param name="numeraireCcy">The numeraire currency.</param>
        /// <param name="rateSimulators"></param>
        /// <param name="currencyPairs"></param>
        /// <param name="spots">The exchange rates at the anchor date.  Discounted from the spot values. Quoted in units of the numeraire currency per unit of the foreign currency.</param>
        /// <param name="vols"></param>
        /// <param name="correlations"></param>
        /// <exception cref="System.ArgumentException">A rate simulator must be provided for the numeraire currency: " + numeraireCcy.ToString()</exception>
        public MultiHWAndFXToy(Date anchorDate, Currency numeraireCcy, HullWhite1F[] rateSimulators,
            CurrencyPair[] currencyPairs,
            double[] spots, double[] vols, double[,] correlations)
        {
            _numeraireCcy = numeraireCcy;
            _currencyPairs = currencyPairs;
            _spots = spots;
            _vols = vols;
            _correlations = correlations;
            _ccySimMap = new Dictionary<string, HullWhite1F>();
            if (rateSimulators == null) return;
            foreach (var simulator in rateSimulators)
                _ccySimMap[simulator.GetNumeraireCurrency().ToString()] = simulator;

            if (!_ccySimMap.ContainsKey(numeraireCcy.ToString()))
                throw new ArgumentException("A rate simulator must be provided for the numeraire currency: " +
                                            numeraireCcy);
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredDates)
        {
            if (index is FloatRateIndex)
            {
                foreach (var rateSimulator in _ccySimMap.Values)
                    if (rateSimulator.ProvidesIndex(index))
                        return rateSimulator.GetIndices(index, requiredDates);
                throw new ArgumentException(index + " is not provided by any of the simulators.");
            }

            if (index is CurrencyPair ccyPair)
            {
                var currencyPairIndex = _currencyPairs.IndexOf(ccyPair);
                var result = new double[requiredDates.Count];
                for (var i = 0; i < requiredDates.Count; i++)
                    if (requiredDates[i] == _anchorDate)
                        result[i] = _spots[currencyPairIndex];
                    else
                        result[i] = _simulation[requiredDates[i]][currencyPairIndex];
                return result;
            }

            throw new ArgumentException(index + " is not provided by this model.");
        }

        public override Currency GetNumeraireCurrency()
        {
            return _numeraireCcy;
        }

        public override double[] GetUnderlyingFactors(Date date)
        {
            var factors = new List<double>();
            foreach (var ccyPair in _currencyPairs)
                factors.Add(GetIndices(ccyPair, new List<Date> {date})[0]);
            foreach (var rateSim in _ccySimMap.Values)
                factors.AddRange(rateSim.GetUnderlyingFactors(date));
            return factors.ToArray();
        }


        public override double Numeraire(Date valueDate)
        {
            return _ccySimMap[_numeraireCcy.ToString()].Numeraire(valueDate);
        }

        public override void Prepare(Date anchorDate)
        {
            _anchorDate = anchorDate;
            foreach (var simulator in _ccySimMap.Values)
                simulator.Prepare(anchorDate);
            _allRequiredDates = _allRequiredDates.Distinct().ToList();
            _allRequiredDates.Sort();
            _normal = new MultivariateNormalDistribution(Vector.Zeros(_currencyPairs.Length), _correlations);
        }

        public override bool ProvidesIndex(MarketObservable index)
        {
            foreach (var simulator in _ccySimMap.Values)
                if (simulator.ProvidesIndex(index))
                    return true;
            if (_currencyPairs.Contains(index))
                return true;
            return false;
        }

        public override void Reset()
        {
            foreach (var simulator in _ccySimMap.Values)
                simulator.Reset();
            _allRequiredDates = new List<Date>();
        }

        public override void RunSimulation(int simNumber)
        {
            foreach (var simulator in _ccySimMap.Values)
                simulator.RunSimulation(simNumber);
            _simulation = new Dictionary<int, double[]>();
            var simPrices = _spots.Copy();
            var oldDrifts = Vector.Ones(simPrices.Length);

            for (var timeCounter = 0; timeCounter < _allRequiredDates.Count; timeCounter++)
            {
                double dt = timeCounter > 0
                    ? _allRequiredDates[timeCounter] - _allRequiredDates[timeCounter - 1]
                    : _allRequiredDates[timeCounter] - _anchorDate.value;
                dt = dt / 365.0;
                var sdt = Math.Sqrt(dt);
                var dW = _normal.Generate();

                for (var s = 0; s < _currencyPairs.Length; s++)
                {
                    var drift = _ccySimMap[_currencyPairs[s].CounterCurrency.ToString()]
                                    .Numeraire(_allRequiredDates[timeCounter]) /
                                _ccySimMap[_currencyPairs[s].BaseCurrency.ToString()]
                                    .Numeraire(_allRequiredDates[timeCounter]);

                    simPrices[s] = simPrices[s] * drift / oldDrifts[s] *
                                   Math.Exp(-0.5 * _vols[s] * _vols[s] * dt + _vols[s] * sdt * dW[s]);
                    oldDrifts[s] = drift;
                }

                _simulation[_allRequiredDates[timeCounter]] = simPrices.Copy();
            }
        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
            foreach (var simulator in _ccySimMap.Values)
                simulator.SetNumeraireDates(requiredDates);
            _allRequiredDates.AddRange(requiredDates);
        }

        public override void SetRequiredDates(MarketObservable index, List<Date> requiredDates)
        {
            foreach (var simulator in _ccySimMap.Values)
                if (simulator.ProvidesIndex(index))
                    simulator.SetRequiredDates(index, requiredDates);
            _allRequiredDates.AddRange(requiredDates);
            //if (index is CurrencyPair)
            //    allRequiredDates.AddRange(requiredDates);
        }
    }
}