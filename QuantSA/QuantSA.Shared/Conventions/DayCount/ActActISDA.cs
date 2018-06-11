using System;
using QuantSA.Shared.Dates;

namespace QuantSA.General.Conventions.DayCount
{
    public class ActActISDA : DayCountConvention
    {
        public static readonly ActActISDA Instance = new ActActISDA();

        private ActActISDA()
        {
        }

        public double YearFraction(Date d1, Date d2)
        {
            if (d1 == d2)
                return 0.0;

            if (d1 > d2)
                return -YearFraction(d2, d1);

            int y1 = d1.Year, y2 = d2.Year;
            var dib1 = DateTime.IsLeapYear(y1) ? 366.0 : 365.0;
            var dib2 = DateTime.IsLeapYear(y2) ? 366.0 : 365.0;

            double sum = y2 - y1 - 1;
            sum += (new Date(y1 + 1, 1, 1) - d1) / dib1;
            sum += (d2 - new Date(y2, 1, 1)) / dib2;
            return sum;
        }
    }
}