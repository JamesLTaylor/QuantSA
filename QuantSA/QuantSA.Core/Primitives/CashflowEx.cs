using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
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

        /// <summary>
        /// Discount and add all the cashflows to the anchor date of <paramref name="discountingSource"/>.
        /// <para>
        /// Assumes all cashflows are in the future and in the same currency as <paramref name="discountingSource"/>.
        /// </para>
        /// </summary>
        /// <param name="cfs"></param>
        /// <param name="discountingSource"></param>
        /// <returns></returns>
        public static double PV(this List<Cashflow> cfs, IDiscountingSource discountingSource)
        {
            var pv = 0.0;
            foreach (var cf in cfs)
            {
                pv += cf.Amount * discountingSource.GetDF(cf.Date);
            }

            return pv;
        }
    }
}