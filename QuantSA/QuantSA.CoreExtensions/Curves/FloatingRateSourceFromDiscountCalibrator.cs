using Newtonsoft.Json;
using QuantSA.Core.MarketData;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.CoreExtensions.Curves
{
    public class FloatingRateSourceFromDiscountCalibrator : IMarketDataSource
    {
        private readonly DiscountingSourceDescription _baseCurveDescription;
        private readonly FloatRateIndex _index;
        private readonly string _name;

        [JsonIgnore] private ForecastCurveFromDiscount _curve;

        public FloatingRateSourceFromDiscountCalibrator(DiscountingSourceDescription baseCurveDescription,
            FloatRateIndex index)
        {
            _baseCurveDescription = baseCurveDescription;
            _index = index;
            _name = new FloatingRateSourceDescription(index).Name;
        }

        public Date GetAnchorDate()
        {
            return _curve?.GetAnchorDate();
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
            if (marketDataDescription.Name == _name) return _curve as T;
            return null;
        }

        public bool TryCalibrate(Date calibrationDate, IMarketDataContainer marketDataContainer)
        {
            if (!marketDataContainer.Contains(_baseCurveDescription)) return false;
            var underlyingDiscountCurve = marketDataContainer.Get(_baseCurveDescription);
            _curve = new ForecastCurveFromDiscount(underlyingDiscountCurve, _index, null);
            return true;
        }
    }
}