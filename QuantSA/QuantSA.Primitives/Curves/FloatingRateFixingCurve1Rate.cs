using QuantSA.General.Dates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
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
