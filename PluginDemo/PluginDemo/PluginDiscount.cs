using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;

namespace PluginDemo
{
    public class PluginDiscount : IDiscountingSource
    {
        private readonly Date _anchorDate;
        private readonly double _rate;

        public PluginDiscount(Date anchorDate, double rate)
        {
            _rate = rate;
            _anchorDate = anchorDate;
        }

        public Date GetAnchorDate()
        {
            return _anchorDate;
        }

        public Currency GetCurrency()
        {
            return Currency.ANY;
        }

        public double GetDF(Date date)
        {
            return 1 / (1 + _rate * (date - _anchorDate) / 365.0);
        }
    }
}
