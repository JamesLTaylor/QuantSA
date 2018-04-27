using QuantSA.Primitives.Dates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
