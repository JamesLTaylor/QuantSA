using System;
using System.Globalization;

namespace QuantSA.Shared.Dates
{
    /// <summary>
    /// Dates should always be thought of as whole numbers in the QuantSA library.  They are treated as doubles
    /// sometimes to make calculations easier but this should just be a double representation of an int value.
    /// 
    /// It is also convenient to test for equality of dates when one is only considering days.
    /// 
    /// Operational requirements such as interacting with exchanges and other "real world" systems and events may
    /// require a more precise definition of dates but this is outside the scope of this valuation library.
    /// </summary>
    /// <remarks>
    /// The choice of Epoch for this class is completely arbitrary and exists only to make sure that calculations 
    /// can easily be performed on number of days and move these backwards and forwards between dates without having to 
    /// make new <see cref="DateTime"/>s.
    /// </remarks>s
    public class Date : IComparable<Date>
    {
        private static readonly DateTime Epoch = new DateTime(2000, 1, 1);
        public static string DefaultFormat = "yyyy-MM-dd";

        /// <summary>
        /// Creates a Date from a value that reflects the number of days since the Epoch for this class.
        /// </summary>
        /// <param name="value"></param>
        public Date(double value)
        {
            this.value = (int) value;
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

        public Date(string dateStr) : this(DateTime.ParseExact(dateStr, DefaultFormat, CultureInfo.InvariantCulture))
        {
        }

        private DateTime date { get; } //private set; }
        public int value { get; }

        public int Day => date.Day;

        public int Month => date.Month;

        public int Year => date.Year;

        /// <summary>
        /// Date in ISO8601 (yyyy-MM-dd) format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return date.ToString(DefaultFormat);
        }

        public static bool IsLeapYear(int y)
        {
            return DateTime.IsLeapYear(y);
        }

        public static int DaysInMonth(int y, int m)
        {
            return DateTime.DaysInMonth(y, m);
        }

        public static Date EndOfMonth(Date d)
        {
            return new Date(d - d.Day + DaysInMonth(d.Year, d.Month));
        }

        public static bool IsEndOfMonth(Date d)
        {
            return d.Day == DaysInMonth(d.Year, d.Month);
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

        public static implicit operator int(Date d)
        {
            return d.value;
        }

        public static implicit operator double(Date d)
        {
            return d.value;
        }

        /// <summary>
        /// Returns a new date <paramref name="days"/> after the current date.  Leaves input date unchanged. 
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public Date AddDays(int days)
        {
            return new Date(date.AddDays(days));
        }

        /// <summary>
        /// Returns a new date <paramref name="months"/> after the current date.  Leaves the date unchanged. 
        /// </summary>
        /// <param name="months"></param>
        /// <returns></returns>
        public Date AddMonths(int months)
        {
            return new Date(date.AddMonths(months));
        }

        /// <summary>
        /// Return a new date with the <paramref name="tenor"/> added to it.
        /// </summary>
        /// <param name="tenor">The amount of time to add to the date.</param>
        /// <returns></returns>
        public Date AddTenor(Tenor tenor)
        {
            var newDate = date.AddYears(tenor.Years);
            newDate = newDate.AddMonths(tenor.Months);
            newDate = newDate.AddDays(tenor.Weeks * 7 + tenor.Days);
            return new Date(newDate);
        }

        #region Comparisons

        public int CompareTo(Date compareDate)
        {
            return value.CompareTo(compareDate.value);
        }

        public static bool operator ==(Date left, Date right)
        {
            if ((object) left == null && (object) right == null) return true;
            if ((object) left != null && (object) right == null) return false;
            if ((object) left == null && (object) right != null) return false;
            return left.GetHashCode() == right.GetHashCode();
        }

        public static bool operator !=(Date left, Date right)
        {
            return !(left == right);
        }

        //TODO: Handle nulls
        public static bool operator <(Date left, Date right)
        {
            return left.value < right.value;
        }

        public static bool operator >(Date left, Date right)
        {
            return left.value > right.value;
        }

        public static bool operator <=(Date left, Date right)
        {
            return left.value <= right.value;
        }

        public static bool operator >=(Date left, Date right)
        {
            return left.value >= right.value;
        }

        public override bool Equals(object obj)
        {
            var d = obj as Date;
            if (d == null) return false;
            return value == d.value;
        }

        public override int GetHashCode()
        {
            return value;
        }

        public object ToOADate()
        {
            return date.ToOADate();
        }

        public DayOfWeek DayOfWeek()
        {
            return date.DayOfWeek;
        }

        #endregion

        /*static public implicit operator Date (double d)
        {
            return new Date(d);
        }*/
    }
}