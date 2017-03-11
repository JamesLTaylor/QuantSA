using System;
using System.Collections.Generic;

namespace QuantSA.General.Dates
{

    /// <summary>
    /// 
    /// This class provides methods for determining whether a date is a
    /// business day or a holiday for a given market, and for
    /// incrementing/decrementing a date of a given number of business days.
    /// 
    /// A calendar should be defined for specific exchange holiday schedule
    /// or for general country holiday schedule. Legacy city holiday schedule
    /// calendars will be moved to the exchange/country convention.
    /// </summary>
    /// <remarks>
    /// This class is based on Calendar in QLNET (https://github.com/amaggiulli/QLNet) 
    /// </remarks>
    public class Calendar
    {
        private List<Date> holidays = new List<Date>();

        public Calendar(List<Date> holidays)
        {
            this.holidays = new List<Date>();
            foreach (Date date in holidays)
                this.holidays.Add(new Date(date));
        }

        /// <summary>
        /// Determines whether the specified date is a business day.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        ///   <c>true</c> if the specified date is neither a holiday nor a Saturday or Sunday; otherwise, <c>false</c>.
        /// </returns>
        public bool isBusinessDay(Date d)
        {

            if (holidays.Contains(d))
                return false;
            return (!isWeekend(d.DayOfWeek()));            
        }


        //<summary>
        // Returns <tt>true</tt> iff the weekday is Saturday or Sunday.
        //</summary>
        public bool isWeekend(DayOfWeek w)
        {
            return w == DayOfWeek.Saturday || w == DayOfWeek.Sunday;
        }

        public bool isHoliday(Date d) { return !isBusinessDay(d); }

        /// <summary>
        /// Returns <tt>true</tt> iff the date is last business day for the
        /// month in given market.
        /// </summary>
        public bool isEndOfMonth(Date d)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// last business day of the month to which the given date belongs
        /// </summary>
        public Date endOfMonth(Date d)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Calculates the number of business days between two given
        /// dates and returns the result.
        /// </summary>
        public int businessDaysBetween(Date from, Date to, bool includeFirst = true, bool includeLast = false)
        {
            int wd = 0;
            if (from != to)
            {
                if (from < to)
                {
                    // the last one is treated separately to avoid incrementing Date::maxDate()
                    for (Date d = from; d < to; d=d.AddDays(1))
                    {
                        if (isBusinessDay(d))
                            ++wd;
                    }
                    if (isBusinessDay(to))
                        ++wd;
                }
                else
                {
                    for (Date d = to; d < from; d=d.AddDays(1))
                    {
                        if (isBusinessDay(d))
                            ++wd;
                    }
                    if (isBusinessDay(from))
                        ++wd;
                }

                if (isBusinessDay(from) && !includeFirst)
                    wd--;
                if (isBusinessDay(to) && !includeLast)
                    wd--;

                if (from > to)
                    wd = -wd;
            }
            return wd;
        }

        /// <summary>
        /// Adds a date to the set of holidays for the given calendar.
        /// </summary>
        public void addHoliday(Date d)
        {
            if (isBusinessDay(d))
                holidays.Add(d);
        }
    }
}
