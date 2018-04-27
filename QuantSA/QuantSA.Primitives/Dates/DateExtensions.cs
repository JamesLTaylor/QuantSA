using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General.Dates
{
    /// <summary>
    /// Extension methods for Dates and arrays of Dates
    /// </summary>
    public static class DateExtensions
    {
        /// <summary>
        /// Returns a copy of the date values.
        /// </summary>
        /// <param name="dates">The array from which values are required.</param>
        /// <returns></returns>
        public static double[] GetValues(this Date[] dates)
        {
            double[] values = new double[dates.Length];
            for (int i = 0; i < dates.Length; i++) { values[i] = dates[i]; }

            return values;
        }

        /// <summary>
        /// Returns a new list of dates with the same date values and order as the original list.
        /// </summary>
        /// <param name="dates">The list to be copied.</param>
        /// <returns></returns>
        public static List<Date> Clone(this List<Date> dates)
        {
            List<Date> newDates = new List<Date>();
            foreach (Date date in dates) newDates.Add(new Date(date));
            return newDates;
        }
    }
}
