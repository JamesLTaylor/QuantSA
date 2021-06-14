using System.Collections.Generic;
using QuantSA.Shared.Conventions.DayCount;
using QuantSA.Shared.Dates;

namespace QuantSA.Core.Dates
{
    public static class DateGenerators
    {
        /// <summary>
        /// Creates <paramref name="numberOfDates" /> that are <paramref name="periodTenor" /> apart.  The first
        /// date is <paramref name="startDate" /> plus <paramref name="periodTenor" />.
        /// <para />
        /// There is no holiday adjustment or stub period.
        /// </summary>
        /// <param name="periodTenor">The period tenor.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="numberOfDates">The number of dates.</param>
        /// <param name="paymentDates"></param>
        /// <param name="accrualFractions"></param>
        /// <returns></returns>
        public static void CreateDatesNoHolidays(Tenor periodTenor, Date startDate, int numberOfDates,
            out Date[] paymentDates, out double[] accrualFractions)
        {
            var runningDate = new Date(startDate);
            paymentDates = new Date[numberOfDates];
            accrualFractions = new double[numberOfDates];
            var oldDate = new Date(startDate);
            for (var i = 0; i < numberOfDates; i++)
            {
                runningDate = runningDate.AddTenor(periodTenor);
                paymentDates[i] = runningDate;
                accrualFractions[i] = (runningDate - oldDate) / 365.0;
                oldDate = runningDate;
            }
        }

        /// <summary>
        /// Creates dates that are <paramref name="periodTenor" /> apart.  The first
        /// date is <paramref name="startDate" /> the last date payment date is the first rolled date on or after
        /// <paramref name="startDate" /> plus <paramref name="endTenor" />.
        /// <para />
        /// There is no holiday adjustment or stub period.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endTenor"></param>
        /// <param name="periodTenor">The period tenor.</param>
        /// <param name="resetDates"></param>
        /// <param name="paymentDates"></param>
        /// <param name="accrualFractions"></param>
        /// <returns></returns>
        public static void CreateDatesNoHolidays(Date startDate, Tenor endTenor, Tenor periodTenor,
            out List<Date> resetDates, out List<Date> paymentDates, out List<double> accrualFractions)
        {
            var dayCount = Actual365Fixed.Instance;
            resetDates = new List<Date>();
            paymentDates = new List<Date>();
            accrualFractions = new List<double>();
            var endDate = startDate.AddTenor(endTenor);
            var resetDate = new Date(startDate);
            var paymentDate = resetDate.AddTenor(periodTenor);
            while (paymentDate <= endDate)
            {
                resetDates.Add(resetDate);
                paymentDates.Add(paymentDate);
                accrualFractions.Add(dayCount.YearFraction(resetDate, paymentDate));
                resetDate = new Date(paymentDate);
                paymentDate = resetDate.AddTenor(periodTenor);
            }
        }

        public static void CreateDatesNoHolidaysASWFixed(Date startDate, Date maturityDate, Tenor endTenor, Tenor periodTenor, //Have added this function here and added maturity Date to it and deleted accrualFractions
                out List<Date> resetDates, out List<Date> paymentDates)
        {
            var dayCount = Actual365Fixed.Instance;
            resetDates = new List<Date>();
            paymentDates = new List<Date>();
            var endDate = maturityDate;
            var resetDate = new Date(startDate);
            var paymentDate = resetDate.AddTenor(periodTenor);
            while (paymentDate <= endDate)
            {
                resetDates.Add(resetDate);
                paymentDates.Add(paymentDate);
                resetDate = new Date(paymentDate);
                paymentDate = resetDate.AddTenor(periodTenor);
            }

        }

        public static void CreateDatesNoHolidaysASWfloat(Date settleDate, Date maturityDate, Tenor endTenor, Tenor periodTenor, //Have added this function here and added maturity Date to it
                out List<Date> resetDates, out List<Date> paymentDates, out List<double> accrualFractions)
        {
            var dayCount = Actual365Fixed.Instance;
            resetDates = new List<Date>();
            paymentDates = new List<Date>();
            accrualFractions = new List<double>();
            var endDate = maturityDate;
            var paymentDate = new Date(endDate);
            var resetDate = paymentDate.SubtractTenor(periodTenor);
            while (resetDate >= settleDate)
            {
                paymentDates.Add(paymentDate);
                resetDates.Add(resetDate);
                accrualFractions.Add(dayCount.YearFraction(resetDate, paymentDate));
                paymentDate = new Date(resetDate);
                resetDate = paymentDate.SubtractTenor(periodTenor);
            }

            resetDates.Reverse();
            paymentDates.Reverse();
            accrualFractions.Reverse();

            resetDates[0] = new Date(settleDate);

        }


    }
}