using System;
using QuantSA.General.Conventions.DayCount;
using QuantSA.Shared.Conventions.Compounding;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Exceptions;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.MarketData
{
    /// <summary>
    ///     A single <see cref="Actual365Fixed" /> rate with <see cref="Continuous" /> compounding to use a curve.
    /// </summary>
    public class SingleRate : IDiscountingSource
    {
        private readonly Currency _currency;
        private readonly double _rate;
        private readonly Date _anchorDate;

        public SingleRate(double rate, Date anchorDate, Currency currency)
        {
            _rate = rate;
            _anchorDate = anchorDate;
            _currency = currency;
        }

        public Currency GetCurrency()
        {
            return _currency;
        }

        public double GetDF(Date date)
        {
            if (date < _anchorDate)
                throw new IndexOutOfRangeException(
                    "Discount factors are only defined at dates on or after the anchor date");
            return Math.Exp(-_rate * (date - _anchorDate) / 365.0);
        }

        public Date GetAnchorDate()
        {
            return _anchorDate;
        }

        public string GetName()
        {
            return new DiscountingSourceDescription(_currency).Name;
        }

        public bool CanBeA<T>(MarketDataDescription<T> marketDataDescription, IMarketDataContainer marketDataContainer)
            where T : class, IMarketDataSource
        {
            return marketDataDescription.Name == new DiscountingSourceDescription(_currency).Name;
        }

        public T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource
        {
            if (marketDataDescription.Name == new DiscountingSourceDescription(_currency).Name) return this as T;
            throw new MissingMarketDataException($"{GetName()} cannot be used as {marketDataDescription.Name}");
        }

        public bool TryCalibrate(Date calibrationDate, IMarketDataContainer marketDataContainer)
        {
            return true;
        }
    }
}