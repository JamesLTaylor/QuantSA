using General;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarlo.Equity
{
    public class SimpleBlackEquity : Simulator
    {
        private Date anchorDate;
        private double divYield;
        private int[] allRequiredTimes;
        private double riskfreeRate;
        private Share share;
        private Dictionary<int, double> simulation;
        private double spotPrice;
        private double vol;

        public SimpleBlackEquity(Date anchorDate, string shareCode, double spotPrice, double vol, double riskfreeRate, double divYield)
        {
            this.anchorDate = anchorDate;
            share = new Share(shareCode);
            this.spotPrice = spotPrice;
            this.vol = vol;
            this.riskfreeRate = riskfreeRate;
            this.divYield = divYield;
        }

        public override double[] GetIndices(MarketObservable index, int[] requiredTimes)
        {
            double[] result = new double[requiredTimes.Length];
            for (int i = 0; i<requiredTimes.Length; i++)
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
            allRequiredTimes = null;
        }


        public override void RunSimulation(int simNumber)
        {
            Normal dist = new Normal();
            simulation = new Dictionary<int, double>();
            double price = spotPrice;

            for (int timeCounter = 0; timeCounter< allRequiredTimes.Length; timeCounter++)
            {
                double dt = timeCounter > 0 ? allRequiredTimes[timeCounter] - allRequiredTimes[timeCounter - 1] : allRequiredTimes[timeCounter] - anchorDate.value;
                dt = dt / 365.0;
                double sdt = Math.Sqrt(dt);
                price = price * Math.Exp((riskfreeRate - divYield - 0.5 * vol * vol) * dt + vol * sdt * dist.Sample());
                simulation[allRequiredTimes[timeCounter]] = price;
            }
            
        }

        public override void SetRequiredTimes(MarketObservable index, int[] requiredTimes)
        {
            this.allRequiredTimes = requiredTimes;
            Array.Sort(this.allRequiredTimes);
        }
    }
}
