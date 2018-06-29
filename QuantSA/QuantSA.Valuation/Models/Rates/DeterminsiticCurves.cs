using System;
using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Valuation
{
    /// <summary>
    /// A "Simulator" that works with only 1 simulation.  The numeraire and all forward rates are obtained 
    /// directly from a curve.
    /// <para/>
    /// Using this model is the equivalent of the usual forecasting and discounting valuation for linear 
    /// instruments like swaps.
    /// </summary>
    /// <seealso cref="QuantSA.Valuation.NumeraireSimulator" />
    public class DeterminsiticCurves : NumeraireSimulator
    {
        private readonly IDiscountingSource discountCurve;
        private readonly Dictionary<string, IFloatingRateSource> forecastCurves;
        private readonly Dictionary<string, IFXSource> fxCurves;
        private readonly Currency numeraireCurrency;

        private DeterminsiticCurves()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterminsiticCurves"/> class with just a discounting curve.
        /// Additional rate forecast curves are added with <see cref="AddRateForecast(IFloatingRateSource)"/>, and 
        /// FXC forecast sources are added with <see cref="AddFXForecast(IFXSource)"/>.
        /// </summary>
        /// <param name="discountCurve">The discount curve.</param>
        public DeterminsiticCurves(IDiscountingSource discountCurve)
        {
            numeraireCurrency = discountCurve.GetCurrency();
            this.discountCurve = discountCurve;
            forecastCurves = new Dictionary<string, IFloatingRateSource>();
            fxCurves = new Dictionary<string, IFXSource>();
        }

        /// <summary>
        /// Adds a source for interest rate forecasts.
        /// </summary>
        /// <param name="forecastCurve">The forecast curve.</param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <seealso cref="AddFXForecast(IFXSource[])"/>
        public void AddRateForecast(IFloatingRateSource forecastCurve)
        {
            if (!forecastCurves.ContainsKey(forecastCurve.GetFloatingIndex().ToString()))
                forecastCurves.Add(forecastCurve.GetFloatingIndex().ToString(), forecastCurve);
            else
                throw new ArgumentException(forecastCurve.GetFloatingIndex() + " has already been added to the model.");
        }

        /// <summary>
        /// Adds an array of interest rate forecast sources.
        /// </summary>
        /// <param name="forecastCurves">The forecast curves.</param>
        /// /// <seealso cref="AddFXForecast(IFXSource)"/>
        public void AddRateForecast(IFloatingRateSource[] forecastCurves)
        {
            foreach (var forecastCurve in forecastCurves)
                AddRateForecast(forecastCurve);
        }


        public void AddFXForecast(IFXSource fxForecastCurve)
        {
            if (numeraireCurrency.GetHashCode() == Currency.ANY.GetHashCode())
                throw new ArgumentException(
                    "If the model provides multiple currencies then the discounting currency cannot be 'ANY'.");
            if (!fxCurves.ContainsKey(fxForecastCurve.GetCurrencyPair().ToString()))
                fxCurves.Add(fxForecastCurve.GetCurrencyPair().ToString(), fxForecastCurve);
            else
                throw new ArgumentException(fxForecastCurve.GetCurrencyPair() +
                                            " has already been added to the model.");
        }

        public void AddFXForecast(IFXSource[] fxForecastCurves)
        {
            foreach (var fxForecastCurve in fxForecastCurves)
                AddFXForecast(fxForecastCurve);
        }

        /// <summary>
        /// Return the simulated values at the required times.  Will only be called after
        /// <see cref="RunSimulation" />
        /// </summary>
        /// <param name="index"></param>
        /// <param name="requiredDates"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">This model instance does not provide values for " + index.ToString()</exception>
        public override double[] GetIndices(MarketObservable index, List<Date> requiredDates)
        {
            var result = new double[requiredDates.Count];
            var i = 0;
            foreach (var date in requiredDates)
            {
                if (index is FloatRateIndex)
                    result[i] = forecastCurves[index.ToString()].GetForwardRate(date);
                else if (index is CurrencyPair)
                    result[i] = fxCurves[index.ToString()].GetRate(date);
                else throw new ArgumentException("This model instance does not provide values for " + index);
                i++;
            }

            return result;
        }

        public override bool ProvidesIndex(MarketObservable index)
        {
            var floatIndex = index as FloatRateIndex;
            if (floatIndex != null) return forecastCurves.ContainsKey(floatIndex.ToString());
            var currencyPair = index as CurrencyPair;
            if (currencyPair != null) return fxCurves.ContainsKey(currencyPair.ToString());
            return false;
        }

        public override void Reset()
        {
        }

        public override void Prepare(Date anchorDate)
        {
        }

        public override void RunSimulation(int simNumber)
        {
        }

        public override void SetRequiredDates(MarketObservable index, List<Date> requiredTimes)
        {
        }

        public override Currency GetNumeraireCurrency()
        {
            return numeraireCurrency;
        }

        public override double Numeraire(Date valueDate)
        {
            return 1 / discountCurve.GetDF(valueDate);
        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
        }

        public override double[] GetUnderlyingFactors(Date date)
        {
            return new double[] {1};
        }
    }
}