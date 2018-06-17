using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Primitives
{
    /// <summary>
    /// Instead of a Cashflows class that contains multiple <see cref="Cashflow"/>s, rather use
    /// a <see cref="List{T}"/> and these extension methods.
    /// </summary>
    public static class CashflowExtensions
    {
        /// <summary>
        /// Return a list with copies of all the dates.  The order of the dates is maintained.
        /// </summary>
        /// <param name="cfs">The list of cashflows.</param>
        /// <returns></returns>
        public static List<Date> GetDates(this List<Cashflow> cfs)
        {
            var dates = new List<Date>();
            foreach (var cf in cfs) dates.Add(new Date(cf.Date));
            return dates;
        }
    }
}