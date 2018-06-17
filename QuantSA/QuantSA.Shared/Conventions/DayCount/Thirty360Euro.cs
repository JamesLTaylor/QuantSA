using System;
using QuantSA.Shared.Conventions.DayCount;
using QuantSA.Shared.Dates;

namespace QuantSA.General.Conventions.DayCount
{
    public class Thirty360Euro : IDayCountConvention
    {
        public static readonly Thirty360Euro Instance = new Thirty360Euro();

        private Thirty360Euro()
        {
        }

        public double YearFraction(Date d1, Date d2)
        {
            int dd1 = d1.Day, dd2 = d2.Day;
            int mm1 = d1.Month, mm2 = d2.Month;
            int yy1 = d1.Year, yy2 = d2.Year;

            return (360 * (yy2 - yy1) + 30 * (mm2 - mm1 - 1) +
                    Math.Max(0, 30 - dd1) + Math.Min(30, dd2)) / 360.0;
        }
    }
}