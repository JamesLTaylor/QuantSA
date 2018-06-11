using System;
using QuantSA.Primitives.Dates;
using QuantSA.Shared.MarketData;

namespace QuantSA.General
{
    [Serializable]
    public class SingleRate : IDiscountingSource
    {
        private readonly Date anchorDate;
        private readonly double rate;

        private SingleRate(double rate, Date anchorDate)
        {
            this.rate = rate;
            this.anchorDate = anchorDate;
        }

        public Date GetAnchorDate()
        {
            return anchorDate;
        }

        public Currency GetCurrency()
        {
            return Currency.ANY;
        }

        public double GetDF(Date date)
        {
            if (date < anchorDate)
                throw new IndexOutOfRangeException(
                    "Discount factors are only defined at dates on or after the anchor date");
            return Math.Exp(-rate * (date - anchorDate) / 365.0);
        }

        public static SingleRate Continuous(double rate, Date anchorDate)
        {
            return new SingleRate(rate, anchorDate);
        }
    }
}