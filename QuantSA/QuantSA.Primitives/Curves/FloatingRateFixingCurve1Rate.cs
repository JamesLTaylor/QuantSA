using System;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.MarketObservables;

namespace QuantSA.Primitives.Curves
{
    /// <summary>
    /// Provides fixes for the past at a single rate.  This should only be used an an expediency, rather use  
    /// </summary>
    [Serializable]
    public class FloatingRateFixingCurve1Rate : IFloatingRateSource
    {
        double rate;
        FloatingIndex index;

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
