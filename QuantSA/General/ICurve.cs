using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    /// <summary>
    /// A curve is simply a function between time and values.
    /// </summary>
    public interface ICurve
    {
        double InterpAtTime(double time);
    }
}
