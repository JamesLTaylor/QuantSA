using QuantSA;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarlo
{
    public class SimpleBlackEquity : Simulator
    {
        private Date anchorDate;
        private double divYield;
        private List<Date> allRequiredDates;
        private double riskfreeRate;
        private Share share;
        private Dictionary<int, double> simulation;
        private double spotPrice;
        private double vol;

        public SimpleBlackEquity(Date anchorDate, string shareCode, double spotPrice, double vol, double riskfreeRate, double divYield)
        {
            this.anchorDate = anchorDate;
            share = new Share(shareCode, Currency.ZAR);
            this.spotPrice = spotPrice;
            this.vol = vol;
            this.riskfreeRate = riskfreeRate;
            this.divYield = divYield;
        }

        public override double[] GetIndices(MarketObservable index, List<Date> requiredTimes)
        {
            double[] result = new double[requiredTimes.Count];
            for (int i = 0; i<requiredTimes.Count; i++)
            {
                result[i] = simulation[requiredTimes[i]];
            }
            return result;
        }

        public override bool ProvidesIndex(MarketObservable index)
        {
            return (index.Equals(share));
            
        }

        public override void Reset()
        {
            allRequiredDates = null;
        }


        public override void RunSimulation(int simNumber)
        {
            Normal dist = new Normal();
            simulation = new Dictionary<int, double>();
            double price = spotPrice;

            for (int timeCounter = 0; timeCounter< allRequiredDates.Count; timeCounter++)
            {
                double dt = timeCounter > 0 ? allRequiredDates[timeCounter] - allRequiredDates[timeCounter - 1] : allRequiredDates[timeCounter] - anchorDate.value;
                dt = dt / 365.0;
                double sdt = Math.Sqrt(dt);
                price = price * Math.Exp((riskfreeRate - divYield - 0.5 * vol * vol) * dt + vol * sdt * dist.Sample());
                simulation[allRequiredDates[timeCounter]] = price;
            }
            
        }

        public override void SetRequiredTimes(MarketObservable index, List<Date> requiredDates)
        {
            allRequiredDates = requiredDates;
            allRequiredDates.Sort();            
        }
    }
}
