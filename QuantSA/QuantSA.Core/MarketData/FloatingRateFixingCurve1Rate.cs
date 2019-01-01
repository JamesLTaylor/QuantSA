using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.General
{
    /// <summary>
    /// Provides fixes for the past at a single rate.  This should only be used an an expediency, rather use
    /// </summary>
    public class FloatingRateFixingCurve1Rate : IFloatingRateSource
    {
        private readonly Date _anchorDate;
        private readonly FloatRateIndex _index;
        private readonly string _name;
        private readonly double _rate;

        public FloatingRateFixingCurve1Rate(Date anchorDate, double rate, FloatRateIndex index)
        {
            _anchorDate = anchorDate;
            _rate = rate;
            _index = index;
            _name = new FloatingRateSourceDescription(index).Name;
        }

        public FloatRateIndex GetFloatingIndex()
        {
            return _index;
        }

        public double GetForwardRate(Date date)
        {
            return _rate;
        }

        public Date GetAnchorDate()
        {
            return _anchorDate;
        }

        public string GetName()
        {
            return _name;
        }

        public bool CanBeA<T>(MarketDataDescription<T> marketDataDescription, IMarketDataContainer marketDataContainer)
            where T : class, IMarketDataSource
        {
            return marketDataDescription.Name == _name;
        }

        public T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource
        {
            return marketDataDescription.Name == _name ? this as T : null;
        }

        public bool TryCalibrate(Date calibrationDate, IMarketDataContainer marketDataContainer)
        {
            return true;
        }
    }
}