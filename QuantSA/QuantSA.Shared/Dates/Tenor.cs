﻿using System;
using System.Text;

namespace QuantSA.Shared.Dates
{
    [Serializable]
    public class Tenor
    {
        public Tenor(int days, int weeks, int months, int years)
        {
            if (days + weeks * 7 >= 10000)
                throw new ArgumentException("Please use months or years rather than more than 10000 days.");
            this.days = days;
            this.weeks = weeks;
            this.months = months;
            this.years = years;
        }

        public int days { get; }
        public int months { get; }
        public int weeks { get; }
        public int years { get; }

        public static Tenor Years(int years)
        {
            return new Tenor(0, 0, 0, years);
        }

        public static Tenor Months(int months)
        {
            return new Tenor(0, 0, months, 0);
        }

        public static Tenor Days(int days)
        {
            return new Tenor(days, 0, 0, 0);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (years > 0) sb.Append(years).Append("Y");
            if (months > 0) sb.Append(months).Append("M");
            if (weeks > 0) sb.Append(weeks).Append("W");
            if (days > 0) sb.Append(days).Append("D");
            return sb.ToString();
        }

        #region Comparisons

        public static bool operator ==(Tenor left, Tenor right)
        {
            if ((object) left == null && (object) right == null) return true;
            if ((object) left != null && (object) right == null) return false;
            if ((object) left == null && (object) right != null) return false;
            return left.days + 7 * left.weeks == right.days + 7 * right.weeks &&
                   left.months + 12 * left.years == right.months + 12 * right.years;
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
            return days + weeks * 7 + (months + years * 12) * 100000;
        }

        #endregion
    }
}