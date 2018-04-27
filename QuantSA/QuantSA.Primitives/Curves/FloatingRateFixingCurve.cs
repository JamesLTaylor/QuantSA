using System;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.MarketObservables;

namespace QuantSA.Primitives.Curves
{
    //TODO: Implement FloatingRateFixingCurve
    public class FloatingRateFixingCurve : IFloatingRateSource
    {
        public FloatingIndex GetFloatingIndex()
        {
            throw new NotImplementedException();
        }

        public double GetForwardRate(Date date)
        {
            throw new NotImplementedException();
        }
    }
}
