using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Statistics.Distributions.Multivariate;
using QuantSA.General;
using QuantSA.General.Dates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Valuation.Models
{
    public class HWParams
    {
        public double meanReversionSpeed;
        public double vol;
    }

    public class MultiHWAndFXToy : NumeraireSimulator
    {
        private List<Date> allRequiredDates; // the set of all dates that will be simulated.
        private readonly Date anchorDate;

        private readonly Dictionary<Currency, HullWhite1F> ccySimMap;
        private readonly double[,] correlations;
        private readonly CurrencyPair[] currencyPairs;
        private readonly MultivariateNormalDistribution normal;
        private readonly Currency numeraireCcy;
        private readonly HullWhite1F numeraireSimulator;
        private readonly HullWhite1F[] rateSimulators;
        private Dictionary<int, double[]> simulation; // stores the simulated spot rates at each required date
        private readonly double[] spots;
        private readonly double[] vols;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiHWAndFXToy"/> class.
        /// </summary>
        /// <param name="anchorDate">The anchor date.</param>
        /// <param name="numeraireCurve">The numeraire curve.</param>
        /// <param name="numeraireHWParams"></param>
        /// <param name="otherCcys">The other currencies that will be simulated.</param>
        /// <param name="otherCcySpots">The exchange rates at the anchor date.  Discounted from the spot values. Quoted in units of the numeraire currency per unit of the foreign currency.</param>
        /// <param name="otherCcyVols"></param>
        /// <param name="otherCcyCurves"></param>
        /// <param name="otherCcyHwParams"></param>
        /// <param name="correlations">The correlation matrix ordered by: numeraireRate, otherCcy1Rate, ..., otherCcyFX1, ...</param>
        /// <exception cref="System.ArgumentException">A rate simulator must be provided for the numeraire currency: " + numeraireCcy.ToString()</exception>
        public MultiHWAndFXToy(Date anchorDate, IDiscountingSource numeraireCurve,
            List<FloatRateIndex> numeraireCcyRequiredIndices, HWParams numeraireHWParams,
            List<Currency> otherCcys, List<double> otherCcySpots, List<double> otherCcyVols,
            List<IDiscountingSource> otherCcyCurves, List<List<FloatRateIndex>> otherCcyRequiredIndices,
            List<HWParams> otherCcyHwParams,
            double[,] correlations)
        {
            this.anchorDate = anchorDate;
            numeraireCcy = numeraireCurve.GetCurrency();

            var rateSimulatorsList = new List<HullWhite1F>();
            ccySimMap = new Dictionary<Currency, HullWhite1F>();
            var rate = -Math.Log(numeraireCurve.GetDF(anchorDate.AddMonths(12)));
            numeraireSimulator = new HullWhite1F(numeraireCcy, numeraireHWParams.meanReversionSpeed,
                numeraireHWParams.vol, rate, rate, anchorDate);
            foreach (var index in numeraireCcyRequiredIndices)
                numeraireSimulator.AddForecast(index);

            rateSimulatorsList.Add(numeraireSimulator);
            ccySimMap[numeraireCcy] = numeraireSimulator;
            for (var i = 0; i < otherCcys.Count; i++)
            {
                rate = -Math.Log(otherCcyCurves[i].GetDF(anchorDate.AddMonths(12)));
                var thisSim = new HullWhite1F(otherCcys[i], otherCcyHwParams[i].meanReversionSpeed,
                    otherCcyHwParams[i].vol, rate, rate, anchorDate);
                foreach (var index in otherCcyRequiredIndices[i])
                    thisSim.AddForecast(index);
                rateSimulatorsList.Add(thisSim);
                ccySimMap[otherCcys[i]] = thisSim;
            }

            currencyPairs = otherCcys.Select(ccy => new CurrencyPair(ccy, numeraireCcy)).ToArray();
            spots = otherCcySpots.ToArray();
            vols = otherCcyVols.ToArray();
            this.correlations = Matrix.Identity(otherCcys.Count);
            normal = new MultivariateNormalDistribution(Vector.Zeros(currencyPairs.Length), correlations);
            rateSimulators = rateSimulatorsList.ToArray();
        }

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
            this.anchorDate = anchorDate;
            this.numeraireCcy = numeraireCcy;
            this.rateSimulators = rateSimulators;
            this.currencyPairs = currencyPairs;
            this.spots = spots;
            this.vols = vols;
            this.correlations = correlations;
            normal = new MultivariateNormalDistribution(Vector.Zeros(currencyPairs.Length), correlations);
            numeraireSimulator = null;
            ccySimMap = new Dictionary<Currency, HullWhite1F>();
            foreach (var simulator in rateSimulators)
            {
                if (simulator.GetNumeraireCurrency() == numeraireCcy) numeraireSimulator = simulator;
                ccySimMap[simulator.GetNumeraireCurrency()] = simulator;
            }

            if (numeraireSimulator == null)
                throw new ArgumentException("A rate simulator must be provided for the numeraire currency: " +
                                            numeraireCcy);
        }

        public override Simulator Clone()
        {
            var rateSimulatorsCopy = rateSimulators.Select(sim => (HullWhite1F) sim.Clone()).ToArray();
            var model = new MultiHWAndFXToy(new Date(anchorDate), numeraireCcy, rateSimulatorsCopy, currencyPairs,
                spots, vols, correlations);
            model.allRequiredDates = allRequiredDates.Clone();
            return model;
        }


        public override double[] GetIndices(MarketObservable index, List<Date> requiredDates)
        {
            if (index is FloatRateIndex)
            {
                foreach (var rateSimulator in rateSimulators)
                    if (rateSimulator.ProvidesIndex(index))
                        return rateSimulator.GetIndices(index, requiredDates);
                throw new ArgumentException(index + " is not provided by any of the simulators.");
            }

            if (index is CurrencyPair)
            {
                var currencyPairIndex = currencyPairs.IndexOf(index);
                var result = new double[requiredDates.Count];
                for (var i = 0; i < requiredDates.Count; i++)
                    if (requiredDates[i] == anchorDate)
                        result[i] = spots[currencyPairIndex];
                    else
                        result[i] = simulation[requiredDates[i]][currencyPairIndex];
                return result;
            }

            throw new ArgumentException(index + " is not provided by this model.");
        }

        public override Currency GetNumeraireCurrency()
        {
            return numeraireCcy;
        }

        public override double[] GetUnderlyingFactors(Date date)
        {
            var factors = new double[currencyPairs.Length + rateSimulators.Length];
            for (var i = 0; i < currencyPairs.Length; i++)
                factors[i] = GetIndices(currencyPairs[i], new List<Date> {date})[0];
            for (var i = 0; i < rateSimulators.Length; i++)
                factors[i + currencyPairs.Length] = rateSimulators[i].GetUnderlyingFactors(date)[0];
            return factors;
        }


        public override double Numeraire(Date valueDate)
        {
            return numeraireSimulator.Numeraire(valueDate);
        }

        public override void Prepare()
        {
            foreach (var simulator in rateSimulators)
                simulator.Prepare();
            allRequiredDates = allRequiredDates.Distinct().ToList();
            allRequiredDates.Sort();
        }

        public override bool ProvidesIndex(MarketObservable index)
        {
            foreach (var simulator in rateSimulators)
                if (simulator.ProvidesIndex(index))
                    return true;
            if (currencyPairs.Contains(index))
                return true;
            return false;
        }

        public override void Reset()
        {
            foreach (var simulator in rateSimulators)
                simulator.Reset();
            allRequiredDates = new List<Date>();
        }

        public override void RunSimulation(int simNumber)
        {
            foreach (var simulator in rateSimulators)
                simulator.RunSimulation(simNumber);
            simulation = new Dictionary<int, double[]>();
            var simPrices = spots.Copy();
            var oldDrifts = Vector.Ones(simPrices.Length);

            for (var timeCounter = 0; timeCounter < allRequiredDates.Count; timeCounter++)
            {
                double dt = timeCounter > 0
                    ? allRequiredDates[timeCounter] - allRequiredDates[timeCounter - 1]
                    : allRequiredDates[timeCounter] - anchorDate.value;
                dt = dt / 365.0;
                var sdt = Math.Sqrt(dt);
                var dW = normal.Generate();

                for (var s = 0; s < currencyPairs.Length; s++)
                {
                    var drift = ccySimMap[currencyPairs[s].counterCurrency].Numeraire(allRequiredDates[timeCounter]) /
                                ccySimMap[currencyPairs[s].baseCurrency].Numeraire(allRequiredDates[timeCounter]);

                    simPrices[s] = simPrices[s] * drift / oldDrifts[s] *
                                   Math.Exp(-0.5 * vols[s] * vols[s] * dt + vols[s] * sdt * dW[s]);
                    oldDrifts[s] = drift;
                }

                simulation[allRequiredDates[timeCounter]] = simPrices.Copy();
            }
        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
            foreach (var simulator in rateSimulators)
                simulator.SetNumeraireDates(requiredDates);
            allRequiredDates.AddRange(requiredDates);
        }

        public override void SetRequiredDates(MarketObservable index, List<Date> requiredDates)
        {
            foreach (var simulator in rateSimulators)
                if (simulator.ProvidesIndex(index))
                    simulator.SetRequiredDates(index, requiredDates);
            allRequiredDates.AddRange(requiredDates);
            //if (index is CurrencyPair)
            //    allRequiredDates.AddRange(requiredDates);
        }
    }
}