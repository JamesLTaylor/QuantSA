using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    public class Tenor
    {
        
        public Tenor(int days, int weeks, int months, int years)
        {
            this.days = days;
            this.weeks = weeks;
            this.months = months;
            this.years = years;
        }

        public static Tenor Years(int years)
        {
            return new Tenor(0, 0, 0, years);
        }

        public static Tenor Months(int months)
        {
            return new Tenor(0, 0, months, 0);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (years > 0) sb.Append(years).Append("Y");
            if (months > 0) sb.Append(months).Append("M");
            if (weeks > 0) sb.Append(weeks).Append("W");
            if (days > 0) sb.Append(days).Append("D");
            return sb.ToString();
        }

        public int days { get; private set; }
        public int months { get; private set; }
        public int weeks { get; private set; }
        public int years { get; private set; }

    }
}
