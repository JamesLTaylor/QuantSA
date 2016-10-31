using System.Collections.Generic;
using System;
using QuantSA.General;

namespace QuantSA.Valuation
{
    public class DeterminsiticCurves : NumeraireSimulator
    {
        private Currency numeraireCurrency;
        private IDiscountingSource discountCurve;
        private Dictionary<MarketObservable, IFloatingRateSource> forecastCurves;
        private Dictionary<MarketObservable, IFXSource> fxCurves;           

        public DeterminsiticCurves(IDiscountingSource discountCurve)
        {
            numeraireCurrency = discountCurve.GetCurrency();
            this.discountCurve = discountCurve;
            forecastCurves = new Dictionary<MarketObservable, IFloatingRateSource>();
            fxCurves = new Dictionary<MarketObservable, IFXSource>();
        }

        public void AddRateForecast(IFloatingRateSource forecastCurve)
        {
            if (!forecastCurves.ContainsKey(forecastCurve.GetFloatingIndex()))
                forecastCurves.Add(forecastCurve.GetFloatingIndex(), forecastCurve);
            else
                throw new ArgumentException(forecastCurve.GetFloatingIndex().ToString() + " has already been added to the model.");            
        }

        public void AddRateForecast(List<IFloatingRateSource> forecastCurves)
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

        public void AddFXForecast(List<IFXSource> fxForecastCurves)
        {
            foreach (IFXSource fxForecastCurve in fxForecastCurves)
                AddFXForecast(fxForecastCurve);
        }

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