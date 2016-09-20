using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    public class SingleRate : IDiscountingSource
    {
        private Date anchorDate;
        private double rate;

        private SingleRate(double rate, Date anchorDate)
        {
            this.rate = rate;
            this.anchorDate = anchorDate;
        }

        public static SingleRate Continuous(double rate, Date anchorDate)
        {
            return new SingleRate(rate, anchorDate);
        }

        public Date getAnchorDate()
        {
            return anchorDate;
        }

        public double GetDF(Date date)
        {
            if (date.date < anchorDate.date) throw new IndexOutOfRangeException("Discount factors are only defined at dates on or after the anchor date");
            return Math.Exp(-rate * (date - anchorDate) / 365.0);
        }
    }
}
