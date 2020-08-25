using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math.Random;
using Accord.Statistics.Distributions.Univariate;
using Newtonsoft.Json;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Valuation.Models.CreditFX
{
    /// <summary>
    /// Provides an FX process and the default event for a single name.
    /// <para/>
    /// Regression variables are FX, defaultedFlag (0,1), 1 year default probability
    /// </summary>
    /// <seealso cref="NumeraireSimulator" />
    public class DeterministicCreditWithFXJump : NumeraireSimulator
    {
        private readonly MarketObservable _currencyPair;
        private readonly IFXSource _fxSource;
        private readonly double _fxVol;
        private readonly double _relJumpSizeInDefault;
        private readonly double _simRecoveryRate;
        private readonly SurvivalProbabilitySource _survivalProbSource;
        private readonly IDiscountingSource _valueCurrencyDiscount;
        [JsonIgnore] private List<Date> _allRequiredDates; // the set of all dates that will be simulated.
        [JsonIgnore] private Date _anchorDate;
        [JsonIgnore] private double _simDefaultTime;
        [JsonIgnore] private Dictionary<int, double> _simulation; // stores the simulated share prices at each required date
        [JsonIgnore] private NormalDistribution _normal;
        [JsonIgnore] private UniformContinuousDistribution _uniform;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterministicCreditWithFXJump"/> class.
        /// </summary>
        /// <param name="survivalProbSource">A curve that provides survival probabilities.  Usually a hazard curve.</param>
        /// <param name="ccyPair">The other currency required in the simulation.  The valuation currency will 
        /// be inferred from the <paramref name="valueCurrencyDiscount"/>.  This value needs to be explicitly set
        /// since <paramref name="fxSource"/> may provide multiple pairs.</param>
        /// <param name="fxSource">The source FX spot and forwards.</param>
        /// <param name="valueCurrencyDiscount">The value currency discount curve.</param>
        /// <param name="fxVol">The FX volatility.</param>
        /// <param name="relJumpSizeInDefault">The relative jump size in default.  For example if the value currency is ZAR and the 
        /// other currency is USD then the FX is modeled as ZAR per USD and in default the FX rate will change to:
        /// rate before default * (1 + relJumpSizeInDefault).</param>
        /// <param name="expectedRecoveryRate">The constant recovery rate that will be assumed to apply in default.</param>
        public DeterministicCreditWithFXJump(SurvivalProbabilitySource survivalProbSource,
            CurrencyPair ccyPair, IFXSource fxSource, IDiscountingSource valueCurrencyDiscount,
            double fxVol, double relJumpSizeInDefault, double expectedRecoveryRate)
        {
            _survivalProbSource = survivalProbSource;
            _fxSource = fxSource;
            _valueCurrencyDiscount = valueCurrencyDiscount;
            _fxVol = fxVol;
            _relJumpSizeInDefault = relJumpSizeInDefault;
            _simRecoveryRate = expectedRecoveryRate;
            _currencyPair = ccyPair;
        }

        [JsonIgnore] private ReferenceEntity RefEntity => _survivalProbSource.GetReferenceEntity();

        /// <summary>
        /// Gets the indices.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="requiredTimes">The required times.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// defaultTime must only be queried with a single date.
        /// or
        /// defaultRecovery must only be queried with a single date.
        /// or
        /// </exception>
        public override double[] GetIndices(MarketObservable index, List<Date> requiredTimes)
        {
            if (index == _currencyPair)
            {
                var result = new double[requiredTimes.Count];
                for (var i = 0; i < requiredTimes.Count; i++)
                    if (requiredTimes[i] <= _anchorDate)
                        result[i] = _fxSource.GetRate(requiredTimes[i]);
                    else
                        result[i] = _simulation[requiredTimes[i]];
                return result;
            }

            if (index is DefaultTime defaultTime && defaultTime.RefEntity == RefEntity)
            {
                if (requiredTimes.Count > 1)
                    throw new ArgumentException("defaultTime must only be queried with a single date.");
                return new[] {_simDefaultTime};
            }

            if (index is DefaultRecovery defaultRecovery && defaultRecovery.RefEntity == RefEntity)
            {
                if (requiredTimes.Count > 1)
                    throw new ArgumentException("defaultRecovery must only be queried with a single date.");
                return new[] {_simRecoveryRate};
            }

            throw new ArgumentException(index + " is not simulated by this model.");
        }

        /// <summary>
        /// Indicate whether the required share price is simulated by this model
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public override bool ProvidesIndex(MarketObservable index)
        {
            if (index == _currencyPair)
                return true;
            if (index is DefaultTime defaultTime && defaultTime.RefEntity == RefEntity)
                return true;
            if (index is DefaultRecovery defaultRecovery && defaultRecovery.RefEntity == RefEntity)
                return true;
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
            _normal = new NormalDistribution();
            _uniform = new UniformContinuousDistribution();
            Generator.Seed = -533776581; // This magic number is: "DeterministicCreditWithFXJump".GetHashCode();
        }

        /// <summary>
        /// Run a simulation and store the results for later use by <see cref="GetIndices(MarketObservable, List{Date})"/>
        /// </summary>
        /// <param name="simNumber"></param>
        public override void RunSimulation(int simNumber)
        {
            _simulation = new Dictionary<int, double>();
            var spot = _fxSource.GetRate(_anchorDate);
            var simRate = spot;
            var oldFxFwd = spot;
            double newFXfwd;


            var hazEst = _survivalProbSource.GetSP(_survivalProbSource.GetAnchorDate().AddTenor(Tenor.FromYears(1)));
            hazEst = -Math.Log(hazEst);
            // Simulate the default
            var tau = _uniform.Generate();
            tau = Math.Log(tau) / -hazEst;
            _simDefaultTime = _anchorDate.value + tau * 365;

            for (var timeCounter = 0; timeCounter < _allRequiredDates.Count; timeCounter++)
            {
                double dt = timeCounter > 0
                    ? _allRequiredDates[timeCounter] - _allRequiredDates[timeCounter - 1]
                    : _allRequiredDates[timeCounter] - _anchorDate.value;
                newFXfwd = _fxSource.GetRate(new Date(_anchorDate.value + dt));

                dt = dt / 365.0;
                var sdt = Math.Sqrt(dt);
                var dW = _normal.Generate();
                // TODO: drift needs to be adjusted for default rate * jump size
                simRate = simRate * newFXfwd / oldFxFwd * Math.Exp(-0.5 * _fxVol * _fxVol * dt + _fxVol * sdt * dW);
                if (_simDefaultTime < _allRequiredDates[timeCounter])
                    _simulation[_allRequiredDates[timeCounter]] = simRate * (1 + _relJumpSizeInDefault);
                else
                    _simulation[_allRequiredDates[timeCounter]] = simRate;
            }
        }

        public override void SetRequiredDates(MarketObservable index, List<Date> requiredDates)
        {
            _allRequiredDates.AddRange(requiredDates);
        }

        public override Currency GetNumeraireCurrency()
        {
            return _valueCurrencyDiscount.GetCurrency();
        }

        public override double Numeraire(Date valueDate)
        {
            return 1.0 / _valueCurrencyDiscount.GetDF(valueDate);
        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
            // Nothing needs to be done since we are using a deterministic curve.
        }

        public override double[] GetUnderlyingFactors(Date date)
        {
            var regressors = new double[3];
            var fxRate = GetIndices(_currencyPair, new List<Date> {date})[0];
            var defaultIndicator = date < _simDefaultTime ? 0.0 : 1.0;
            var fwdDefaultP = 1.0 - _survivalProbSource.GetSP(date.AddTenor(Tenor.FromYears(1))) /
                              _survivalProbSource.GetSP(date);
            regressors[0] = fxRate;
            regressors[1] = defaultIndicator;
            regressors[2] = fwdDefaultP;
            return regressors;
        }
    }
}