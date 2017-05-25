using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.General.Dates;

namespace QuantSA.General
{
    public interface IFloatingRateSource
    {
        /// <summary>
        /// The forward rate that applies on <paramref name="date"/>.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        double GetForwardRate(Date date);

        /// <summary>
        /// The <see cref="FloatingIndex"/> that this curve forecasts.
        /// </summary>
        /// <returns></returns>
        FloatingIndex GetFloatingIndex();
    }
}
