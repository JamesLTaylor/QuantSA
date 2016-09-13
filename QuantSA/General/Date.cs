using System;

namespace QuantSA
{
    /// <summary>
    /// Dates should always be thought of as whole numbers in the QuantSA library.  They are treated as doubles
    /// sometimes to make calculations easier but this should just be a double representation of an int value.
    /// </summary>
    /// <remarks>
    /// The choice of Epoch for this class is completely arbitrary and exists only to make sure that calculations 
    /// can easily be performed on number of days and move these backwards and forwards between dates without having to 
    /// make new <see cref="DateTime"/>s.
    /// </remarks>
    [Serializable]
    public class Date
    {
        private static DateTime Epoch = new DateTime(2000,1,1);
        public DateTime date { get; private set; }
        public int value { get; private set; }

        /// <summary>
        /// Creates a Date from a value that reflects the number of days since the Epoch for this class.
        /// </summary>
        /// <param name="value"></param>
        public Date(double value)
        {
            this.value = (int)value;
            date = Epoch.AddDays(this.value);
        }


        public Date(DateTime date)
        {
            this.date = date.Date;
            value = (this.date - Epoch).Days;
        }

        public Date(int year, int month, int day) : this(new DateTime(year, month, day).Date)
        {            
        }

        public Date(Date currentDate) : this(currentDate.date)
        {
        }

        /// <summary>
        /// Number of whole calendar days from d1 to d2
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static int operator -(Date d1, Date d2)
        {
            return (d1.date - d2.date).Days;
        }

        static public implicit operator int (Date d)
        {
            return d.value;
        }

        static public implicit operator double (Date d)
        {
            return d.value;
        }

        public Date AddMonths(int months)
        {
            return new Date(date.AddMonths(months));
        }

        /*static public implicit operator Date (double d)
        {
            return new Date(d);
        }*/

    }

    /// <summary>
    /// Extansion methods for Dates and arrays of Dates
    /// </summary>
    public static class DateExtensionMethods
    {
        /// <summary>
        /// Returns a copy of the date values.
        /// </summary>
        /// <param name="dates">The array from which values are required.</param>
        /// <returns></returns>
        public static double[] GetValues(this Date[] dates)
        {
            double[] values = new double[dates.Length];
            for (int i = 0; i < dates.Length; i++) { values[i] = dates[i]; }

            return values;
        }
    }
}