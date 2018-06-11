using System;
using QuantSA.Primitives.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.General
{
    /// <summary>
    /// Provides fixes for the past at a single rate.  This should only be used an an expediency, rather use  
    /// </summary>
    [Serializable]
    public class FloatingRateFixingCurve1Rate : IFloatingRateSource
    {
        private readonly FloatRateIndex index;
        private readonly double rate;

        public FloatingRateFixingCurve1Rate(double rate, FloatRateIndex index)
        {
            this.rate = rate;
            this.index = index;
        }

        public FloatRateIndex GetFloatingIndex()
        {
            return index;
        }

        public double GetForwardRate(Date date)
        {
            return rate;
        }
    }
}