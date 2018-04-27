using QuantSA.Primitives.Dates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Primitives.Dates;

namespace QuantSA.General
{
    public interface IDiscountingSource
    {
        /// <summary>
        /// The earliest date after which discount factors can be obtained.
        /// </summary>
        /// <returns></returns>
        Date GetAnchorDate();

        /// <summary>
        /// The currency of cashflows for which this curve should be used for discounting.
        /// </summary>
        /// <returns></returns>
        Currency GetCurrency();

        /// <summary>
        /// The discount factor from the curves natural anchor date to the provided date.
        /// </summary>
        /// <param name="date">Date on which the discount factor is required.</param>
        /// <returns></returns>
        double GetDF(Date date);
    }
}
