using System.Collections.Generic;
using System;
using QuantSA.General;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.Dates;

namespace QuantSA.Valuation
{
    /// <summary>
    /// A "Simulator" that works with only 1 simulation.  The numeraire and all forward rates are obtained 
    /// directly from a curve.
    /// <para/>
    /// Using this model is the equivalent of the usual forecasting and discounting valuetion for linear 
    /// instrments like swaps.
    /// </summary>
    /// <seealso cref="QuantSA.Valuation.NumeraireSimulator" />
    [Serializable]
    public class DeterminsiticCurves : NumeraireSimulator
    {
        private Currency numeraireCurrency;
        private IDiscountingSource discountCurve;
        private Dictionary<MarketObservable, IFloatingRateSource> forecastCurves;
        private Dictionary<MarketObservable, IFXSource> fxCurves;

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
            forecastCurves = new Dictionary<MarketObservable, IFloatingRateSource>();
            fxCurves = new Dictionary<MarketObservable, IFXSource>();
        }

        /// <summary>
        /// Adds a source for interest rate forecasts.
        /// </summary>
        /// <param name="forecastCurve">The forecast curve.</param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <seealso cref="AddFXForecast(IFXSource[])"/>
        public void AddRateForecast(IFloatingRateSource forecastCurve)
        {
            if (!forecastCurves.ContainsKey(forecastCurve.GetFloatingIndex()))
                forecastCurves.Add(forecastCurve.GetFloatingIndex(), forecastCurve);
            else
                throw new ArgumentException(forecastCurve.GetFloatingIndex().ToString() + " has already been added to the model.");            
        }

        /// <summary>
        /// Adds an array of interest rate forecast sources.
        /// </summary>
        /// <param name="forecastCurves">The forecast curves.</param>
        /// /// <seealso cref="AddFXForecast(IFXSource)"/>
        public void AddRateForecast(IFloatingRateSource[] forecastCurves)
        {
            foreach (IFloatingRateSource forecastCurve in forecastCurves)
                AddRateForecast(forecastCurve);                
        }


        public void AddFXForecast(IFXSource fxForecastCurve)
        {
            if (numeraireCurrency.GetHashCode() == Currency.ANY.GetHashCode())
                throw new ArgumentException("If the model provides multiple currencies then the discounting currency cannot be 'ANY'.");
            if (!fxCurves.ContainsKey(fxForecastCurve.GetCurrencyPair()))
                fxCurves.Add(fxForecastCurve.GetCurrencyPair(), fxForecastCurve);
            else
                throw new ArgumentException(fxForecastCurve.GetCurrencyPair().ToString() + " has already been added to the model.");
        }

        public void AddFXForecast(IFXSource[] fxForecastCurves)
        {
            foreach (IFXSource fxForecastCurve in fxForecastCurves)
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
            double[] result = new double[requiredDates.Count];
            int i = 0;
            foreach (Date date in requiredDates)
            {
                if (index is FloatingIndex)
                {
                    result[i] = forecastCurves[index].GetForwardRate(date);
                }
                else if (index is CurrencyPair)
                {
                    result[i] = fxCurves[index].GetRate(date);
                }
                else throw new ArgumentException("This model instance does not provide values for " + index.ToString());
                i++;
            }
            return result;            
        }

        public override bool ProvidesIndex(MarketObservable index)
        {
            FloatingIndex floatIndex = index as FloatingIndex;
            if (floatIndex != null) return forecastCurves.ContainsKey(floatIndex);
            CurrencyPair currencyPair = index as CurrencyPair;
            if (currencyPair != null) return fxCurves.ContainsKey(currencyPair);
            return false;
        }

        public override void Reset()
        {
            // Do nothing
        }

        public override void Prepare()
        {
            // Do nothing
        }

        public override void RunSimulation(int simNumber)
        {
            // Do nothing
        }

        public override void SetRequiredDates(MarketObservable index, List<Date> requiredTimes)
        {
            // Do nothing
        }

        public override Currency GetNumeraireCurrency()
        {
            return numeraireCurrency;
        }

        public override double Numeraire(Date valueDate)
        {
            return 1/discountCurve.GetDF(valueDate);
        }

        public override void SetNumeraireDates(List<Date> requiredDates)
        {
            // Do nothing
        }

        public override double[] GetUnderlyingFactors(Date date)
        {
            return new double[] { 1 };
        }
    }
}