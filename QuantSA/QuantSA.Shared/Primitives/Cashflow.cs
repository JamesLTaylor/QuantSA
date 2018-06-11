using System;
using System.Collections.Generic;
using QuantSA.General;
using QuantSA.Primitives.Dates;

namespace QuantSA.Shared.Primitives
{
    [Serializable]
    public class Cashflow
    {
        public Cashflow(Date date, double amount, Currency currency)
        {
            Date = date;
            Amount = amount;
            Currency = currency;
        }

        public double Amount { get; }
        public Currency Currency { get; }
        public Date Date { get; }
    }

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