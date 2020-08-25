using System.Collections.Generic;
using QuantSA.Shared.Dates;

namespace QuantSA.Core.Dates
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
            var values = new double[dates.Length];
            for (var i = 0; i < dates.Length; i++) values[i] = dates[i];

            return values;
        }

        /// <summary>
        /// Returns a new list of dates with the same date values and order as the original list.
        /// </summary>
        /// <param name="dates">The list to be copied.</param>
        /// <returns></returns>
        public static List<Date> Clone(this List<Date> dates)
        {
            var newDates = new List<Date>();
            foreach (var date in dates) newDates.Add(new Date(date));
            return newDates;
        }
    }
}