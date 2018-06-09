using System;
using System.Collections.Generic;
using System.IO;
using QuantSA.General.Dates;

namespace QuantSA.ExcelFunctions
{
    public static class StaticData
    {
        private static readonly Dictionary<string, Calendar> Calendars = new Dictionary<string, Calendar>();
        private static readonly object CalendarLock = new object();


        /// <summary>
        /// Gets a calendar from the holiday list in the excel folder.
        /// </summary>
        /// <param name="countryCode">The country or country and exchange code of the calendar required.</param>
        /// <returns></returns>
        public static Calendar GetCalendar(string countryCode)
        {
            lock (CalendarLock)
            {
                if (Calendars.ContainsKey(countryCode))
                    return Calendars[countryCode];

                var path = AppDomain.CurrentDomain.BaseDirectory + "/StaticData/Holidays/" + countryCode + ".csv";
                if (!File.Exists(path)) throw new Exception("The holiday file: " + path + " does not exist.");
                Calendars[countryCode] = Calendar.FromFile(path);
                return Calendars[countryCode];
            }
        }
    }
}