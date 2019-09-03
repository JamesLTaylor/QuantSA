using System;
using System.Globalization;

namespace QSALite.Dates
{
    public struct Date
    {
        public readonly int SerialNumber;

        public Date(string isoDateString)
        {
            SerialNumber = (int) DateTime.ParseExact(isoDateString,
                "yyyy-MM-dd", CultureInfo.InvariantCulture).ToOADate();
        }
    }
}
