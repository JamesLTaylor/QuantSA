using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    public interface IDiscountingSource
    {
        /// <summary>
        /// The earliest date after which discount factors can be obtained.
        /// </summary>
        /// <returns></returns>
        Date getAnchorDate();

        /// <summary>
        /// The discount factor from the curves natural anchor date to the provided date.
        /// </summary>
        /// <param name="date">Date on which the discount factor is required.</param>
        /// <returns></returns>
        double GetDF(Date date);
    }
}
