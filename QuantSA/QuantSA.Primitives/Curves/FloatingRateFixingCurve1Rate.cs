using System;
using QuantSA.Primitives.Dates;

namespace QuantSA.General
{
    /// <summary>
    /// Provides fixes for the past at a single rate.  This should only be used an an expediency, rather use  
    /// </summary>
    [Serializable]
    public class FloatingRateFixingCurve1Rate : IFloatingRateSource
    {
        private readonly FloatingIndex index;
        private readonly double rate;

        public FloatingRateFixingCurve1Rate(double rate, FloatingIndex index)
        {
            this.rate = rate;
            this.index = index;
        }

        public FloatingIndex GetFloatingIndex()
        {
            return index;
        }

        public double GetForwardRate(Date date)
        {
            return rate;
        }
    }
}