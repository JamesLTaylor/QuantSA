using System;
using QuantSA.Primitives.Dates;

namespace QuantSA.General
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