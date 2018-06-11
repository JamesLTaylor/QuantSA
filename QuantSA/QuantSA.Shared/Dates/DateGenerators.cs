using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Primitives.Dates;

namespace QuantSA.General.Dates
{
    public static class DateGenerators
    {
        /// <summary>
        /// Creates <paramref name="numberOfDates"/> that are <paramref name="periodTenor"/> apart.  The first 
        /// date is <paramref name="startDate"/> plus <paramref name="periodTenor"/>.
        /// <para/>
        /// There is no holiday adjustment or stub period.
        /// </summary>
        /// <param name="periodTenor">The period tenor.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="numberOfDates">The number of dates.</param>
        /// <returns></returns>
        public static void CreateDatesNoHolidays(Tenor periodTenor, Date startDate, int numberOfDates,
            out Date[] paymentDates, out double[] accrualFractions)
        {
            Date runningDate = new Date(startDate);
            paymentDates = new Date[numberOfDates];
            accrualFractions = new double[numberOfDates];
            Date oldDate = new Date(startDate);
            for (int i = 0; i < numberOfDates; i++)
            {
                runningDate = runningDate.AddTenor(periodTenor);
                paymentDates[i] = runningDate;
                accrualFractions[i] = (runningDate - oldDate) / 365.0;
                oldDate = runningDate;
            }            
        }
    }
}
