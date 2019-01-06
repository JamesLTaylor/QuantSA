using System.Runtime.InteropServices;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;

namespace PluginDemo
{
    public class PluginDiscount : IDiscountingSource
    {
        private readonly Date _anchorDate;
        private readonly double _rate;
        private readonly Currency _currency;
        private readonly string _name;

        public PluginDiscount(Date anchorDate, double rate, Currency currency)
        {
            _rate = rate;
            _currency = currency;
            _anchorDate = anchorDate;
            _name = new DiscountingSourceDescription(_currency).Name;
        }

        public Date GetAnchorDate()
        {
            return _anchorDate;
        }

        public string GetName()
        {
            return _name;
        }

        public bool CanBeA<T>(MarketDataDescription<T> marketDataDescription, IMarketDataContainer marketDataContainer) where T : class, IMarketDataSource
        {
            return (marketDataDescription.Name == _name);
        }

        public T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource
        {
            if (marketDataDescription.Name == _name) return this as T;
            return null;
        }

        public bool TryCalibrate(Date calibrationDate, IMarketDataContainer marketDataContainer)
        {
            return true;
        }

        public Currency GetCurrency()
        {
            return _currency;
        }

        public double GetDF(Date date)
        {
            return 1 / (1 + _rate * (date - _anchorDate) / 365.0);
        }
    }
}
