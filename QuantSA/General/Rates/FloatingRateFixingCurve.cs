using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
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
