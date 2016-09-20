using MonteCarlo;
using System.Collections.Generic;
using System;

namespace QuantSA
{
    public class DeterminsiticCurves : NumeraireSimulator
    {
        private Currency numeraireCurrency;
        private IDiscountingSource discountCurve;
        private Dictionary<MarketObservable, IFloatingRateSource> forecastCurves;

        public DeterminsiticCurves(Currency currency, IDiscountingSource discountCurve)
        {
            numeraireCurrency = currency;
            this.discountCurve = discountCurve;
            forecastCurves = new Dictionary<MarketObservable, IFloatingRateSource>();
        }

        public void AddForecast(IFloatingRateSource forecastCurve)
        {
            forecastCurves.Add(forecastCurve.GetFloatingIndex(), forecastCurve);
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredDates)
        {
            double[] result = new double[requiredDates.Count];
            int i = 0;
            foreach (Date date in requiredDates)
            {
                result[i] = forecastCurves[index].GetForwardRate(date);
                i++;
            }
            return result;            
        }

        public override bool ProvidesIndex(MarketObservable index)
        {
            FloatingIndex floatIndex = index as FloatingIndex;
            if (floatIndex == null) return false;
            return forecastCurves.ContainsKey(floatIndex);
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

        public override void SetRequiredTimes(MarketObservable index, List<Date> requiredTimes)
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
    }
}