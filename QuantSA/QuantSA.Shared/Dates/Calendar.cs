using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using QuantSA.Shared.Serialization;

namespace QuantSA.Shared.Dates
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
    public class Calendar : SerializableViaName
    {
        private readonly List<Date> _holidays;
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="Calendar"/> class with no public holidays only weekends.
        /// </summary>        
        public Calendar(string name)
        {
            _name = name;
            _holidays = GetHolidays(name);
        }

        public Calendar(string name, IEnumerable<Date> holidays)
        {
            _name = name;
            _holidays = GetHolidays(name);
            foreach (var date in holidays)
                _holidays.Add(new Date(date));
        }

        /// <summary>
        /// Determines whether the specified date is a business day.  That means not a holiday and not a weekend.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        ///   <c>true</c> if the specified date is neither a holiday nor a Saturday or Sunday; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBusinessDay(Date date)
        {
            if (_holidays.Contains(date))
                return false;
            return !IsWeekend(date.DayOfWeek());
        }


        //<summary>
        // Returns <tt>true</tt> iff the weekday is Saturday or Sunday.
        //</summary>
        public static bool IsWeekend(DayOfWeek w)
        {
            return w == DayOfWeek.Saturday || w == DayOfWeek.Sunday;
        }

        /// <summary>
        /// Determines whether the specified date is a holiday.  Does not check for weekends.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified d is holiday; otherwise, <c>false</c>.
        /// </returns>
        public bool IsHoliday(Date date)
        {
            return _holidays.Contains(date);
        }

        /// <summary>
        /// Returns <tt>true</tt> iff the date is last business day for the
        /// month in given market.
        /// </summary>
        public bool IsEndOfMonth(Date d)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// last business day of the month to which the given date belongs
        /// </summary>
        public Date EndOfMonth(Date d)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Calculates the number of business days between two given
        /// dates and returns the result.
        /// </summary>
        public int BusinessDaysBetween(Date from, Date to, bool includeFirst = true, bool includeLast = false)
        {
            var wd = 0;
            if (from != to)
            {
                if (from < to)
                {
                    // the last one is treated separately to avoid incrementing Date::maxDate()
                    for (var d = from; d < to; d = d.AddDays(1))
                        if (IsBusinessDay(d))
                            ++wd;
                    if (IsBusinessDay(to))
                        ++wd;
                }
                else
                {
                    for (var d = to; d < from; d = d.AddDays(1))
                        if (IsBusinessDay(d))
                            ++wd;
                    if (IsBusinessDay(from))
                        ++wd;
                }

                if (IsBusinessDay(from) && !includeFirst)
                    wd--;
                if (IsBusinessDay(to) && !includeLast)
                    wd--;

                if (from > to)
                    wd = -wd;
            }

            return wd;
        }

        /// <summary>
        /// Adds a date to the set of holidays for the given calendar.
        /// </summary>
        public void AddHoliday(Date d)
        {
            if (IsBusinessDay(d))
                _holidays.Add(d);
        }

        public override string GetName()
        {
            return _name;
        }

        /// <summary>
        /// Takes in a string an retrieves the holidays from the from the xml file if it exists.
        /// </summary>
        public List<Date> GetHolidays (string calendarName)
        {
            var _calendar = new List<Date>();
            var path = Directory.GetParent(System.IO.Directory.GetCurrentDirectory())
                .Parent.Parent.Parent.FullName + $"\\QuantSA.Shared\\CalendarData\\{calendarName}Calendar.csv";

            foreach (var date in File.ReadLines(path).ToList())
            {
                _calendar.Add(new Date(date));
            }

            return _calendar;
        }
    }
}