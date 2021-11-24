using QuantSA.Shared.Dates;
using System;

namespace QuantSA.Core.Formulae
{
    public static class LaggedCPI
    {
        /// <summary>
        /// The Lagged CPI Inflation formula
        /// </summary>
        /// <param name="cpiDate">The relevant CPI date for which lagged inflation is being computed.</param>
        /// <param name="cpiDates">The array containing CPI dates over a period.</param>
        /// <param name="cpiRates">The array containing CPI rates over a period.</param>
        /// <returns></returns>
        public static double GetCPI(Date cpiDate, Date[] cpiDates, double[] cpiRates)
        {
            var indexOfCpiDate = Array.IndexOf(cpiDates, new Date(cpiDate.Year, cpiDate.Month, 1));

            if ((indexOfCpiDate < 0) || (indexOfCpiDate > cpiDates.Length))
                throw new ArgumentException("cpiDate is not found in the range of dates provided");
            
            var actualDayInMonth = cpiDate.Day;
            var noDaysInMonth = Date.DaysInMonth(cpiDate.Year, cpiDate.Month);

            var cpi_M4 = (double)(noDaysInMonth - actualDayInMonth + 1) / noDaysInMonth * cpiRates[indexOfCpiDate - 4];
            var cpi_M3 = (double)(actualDayInMonth - 1) / noDaysInMonth * cpiRates[indexOfCpiDate - 3];
            var laggedCPI = cpi_M4 + cpi_M3;

            return laggedCPI;
        }
    }
}