using System.Collections.Generic;
using QuantSA.Shared.MarketData;

namespace QuantSA.Core.MarketData
{
    public class MarketDataContainer : IMarketDataContainer
    {
        private readonly Dictionary<string, IMarketDataSource> _curves = new Dictionary<string, IMarketDataSource>();

        public bool TryGet<T>(MarketDataDescription<T> marketDataDescription, out T curve) where T : class, IMarketDataSource
        {
            curve = _curves[marketDataDescription.Name] as T;
            return curve != null;
        }
    }
}