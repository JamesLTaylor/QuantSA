using System;

namespace QuantSA.Shared.MarketData
{
    public abstract class MarketDataDescription<T> : IEquatable<object>
        where T : class, IMarketDataSource
    {
        public abstract string Name { get; }
    }
}