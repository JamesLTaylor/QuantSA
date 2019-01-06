using System;
using Newtonsoft.Json;

namespace QuantSA.Shared.MarketData
{
    public abstract class MarketDataDescription<T> : IEquatable<object>
        where T : class, IMarketDataSource
    {
        [JsonIgnore] public abstract string Name { get; }
    }
}