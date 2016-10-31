using System;
using System.Collections.Generic;

namespace QuantSA.General
{
    [Serializable]
    public class Cashflow
    {
        public double amount { get; private set; }
        public Currency currency { get; private set; }
        public Date date { get; private set; }

        public Cashflow(Date date, double amount, Currency currency)
        {
            this.date = date;
            this.amount = amount;
            this.currency = currency;
        }
    }

    /// <summary>
    /// Instead of a Cashflows class that contains multiple <see cref="Cashflow"/>s, rather use
    /// a <see cref="List{Cashflow}"/> and these extension methods.
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
            List<Date> dates = new List<Date>();
            foreach (Cashflow cf in cfs)
            {
                dates.Add(new Date(cf.date));
            }
            return dates;
        }
    }
}
