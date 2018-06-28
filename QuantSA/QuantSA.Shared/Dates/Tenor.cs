using System;
using System.Text;

namespace QuantSA.Shared.Dates
{
    
    public class Tenor
    {
        public Tenor(int days, int weeks, int months, int years)
        {
            if (days + weeks * 7 >= 10000)
                throw new ArgumentException("Please use months or years rather than more than 10000 days.");
            Days = days;
            Weeks = weeks;
            Months = months;
            Years = years;
        }

        public int Days { get; }
        public int Months { get; }
        public int Weeks { get; }
        public int Years { get; }

        public static Tenor FromYears(int years)
        {
            return new Tenor(0, 0, 0, years);
        }

        public static Tenor FromMonths(int months)
        {
            return new Tenor(0, 0, months, 0);
        }

        public static Tenor FromDays(int days)
        {
            return new Tenor(days, 0, 0, 0);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Years > 0) sb.Append(Years).Append("Y");
            if (Months > 0) sb.Append(Months).Append("M");
            if (Weeks > 0) sb.Append(Weeks).Append("W");
            if (Days > 0) sb.Append(Days).Append("D");
            return sb.ToString();
        }

        #region Comparisons

        public static bool operator ==(Tenor left, Tenor right)
        {
            if ((object) left == null && (object) right == null) return true;
            if ((object) left != null && (object) right == null) return false;
            if ((object) left == null && (object) right != null) return false;
            return left.Days + 7 * left.Weeks == right.Days + 7 * right.Weeks &&
                   left.Months + 12 * left.Years == right.Months + 12 * right.Years;
        }

        public static bool operator !=(Tenor left, Tenor right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            var t = obj as Tenor;
            if (t == null) return false;
            return this == t;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 1 month can alias as 100000 days.        
        /// The checks on the constructor will prevent this.
        /// </remarks>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Days + Weeks * 7 + (Months + Years * 12) * 100000;
        }

        #endregion
    }
}