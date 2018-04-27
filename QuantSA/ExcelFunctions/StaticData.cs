using System;
using System.Collections.Generic;
using System.IO;
using QuantSA.Primitives.Dates;

namespace QuantSA.ExcelFunctions
{
    public static class StaticData
    {
        private static Dictionary<string, Calendar> calendars = new Dictionary<string, Calendar>();
        private static object calendarLock = new object();


        /// <summary>
        /// Gets a calendar from the holiday list in the excel folder.
        /// </summary>
        /// <param name="countryCode">The country or country and exchange code of the calendar required.</param>
        /// <returns></returns>
        public static Calendar GetCalendar(string code)
        {
            lock (calendarLock)
            {
                if (calendars.ContainsKey(code))
                    return calendars[code];

                string path = AppDomain.CurrentDomain.BaseDirectory.ToString() + "/StaticData/Holidays/" + code + ".csv";
                if (!File.Exists(path)) throw new Exception("The holiday file: " + path + " does not exist.");
                calendars[code] =  Calendar.FromFile(path);
                return calendars[code];                
            }
        }
    }
}
