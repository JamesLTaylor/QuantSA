using System.Collections.Generic;
using System.Linq;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Exceptions;
using QuantSA.Shared.MarketData;

namespace QuantSA.Core.MarketData
{
    public class MarketDataContainer : IMarketDataContainer
    {
        private readonly List<IMarketDataSource> _curves = new List<IMarketDataSource>();

        public void Set(IMarketDataSource curve)
        {
            _curves.Add(curve);
        }

        public void Calibrate(Date calibrationDate)
        {
            foreach (var marketDataSource in _curves)
            {
                marketDataSource.TryCalibrate(calibrationDate, this);
            }
        }

        public bool Contains<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource
        {
            return _curves.Any(c => c.CanBeA(marketDataDescription, this));
        }

        public T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource
        {
            T foundCurve = null;
            foreach (var marketDataSource in _curves)
            {
                if (marketDataSource.CanBeA(marketDataDescription, this))
                {
                    if (foundCurve == null)
                    {
                        foundCurve = marketDataSource as T;
                    }
                    else
                        throw new MissingMarketDataException($"At least two curves provide the same description: {foundCurve.GetName()} and {marketDataSource.GetName()}.");
                }

            }
            if (foundCurve==null)
                throw new MissingMarketDataException($"There is no market data: {marketDataDescription.Name}");
            return foundCurve;
        }
    }
}