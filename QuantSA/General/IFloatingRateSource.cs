using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    public interface IFloatingRateSource
    {
        double GetForwardRate(Date date);
    }
}
