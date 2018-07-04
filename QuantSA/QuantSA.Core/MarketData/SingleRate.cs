using System;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.MarketData
{
    public class SingleRate : IDiscountingSource
    {
        private readonly Date anchorDate;
        private readonly Currency _ccy;
        private readonly double rate;

        private SingleRate(double rate, Date anchorDate, Currency ccy)
        {
            this.rate = rate;
            this.anchorDate = anchorDate;
            _ccy = ccy;
        }

        public Date GetAnchorDate()
        {
            return anchorDate;
        }

        public Currency GetCurrency()
        {
            return _ccy;
        }

        public double GetDF(Date date)
        {
            if (date < anchorDate)
                throw new IndexOutOfRangeException(
                    "Discount factors are only defined at dates on or after the anchor date");
            return Math.Exp(-rate * (date - anchorDate) / 365.0);
        }
    }
}