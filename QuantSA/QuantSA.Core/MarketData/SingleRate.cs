using System;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.MarketData
{
    public class SingleRate : IDiscountingSource
    {
        private readonly Date _anchorDate;
        private readonly Currency _ccy;
        private readonly double _rate;

        private SingleRate(double rate, Date anchorDate, Currency ccy)
        {
            _rate = rate;
            _anchorDate = anchorDate;
            _ccy = ccy;
        }

        public Currency GetCurrency()
        {
            return _ccy;
        }

        public double GetDF(Date date)
        {
            if (date < _anchorDate)
                throw new IndexOutOfRangeException(
                    "Discount factors are only defined at dates on or after the anchor date");
            return Math.Exp(-_rate * (date - _anchorDate) / 365.0);
        }

        public Date AnchorDate => _anchorDate;
    }
}