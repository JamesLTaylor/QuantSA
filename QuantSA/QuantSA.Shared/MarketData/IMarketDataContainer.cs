namespace QuantSA.Shared.MarketData
{
    /// <summary>
    /// A collection of <see cref="IMarketDataSource"/>s.
    /// </summary>
    public interface IMarketDataContainer
    {
        bool TryGet<T>(MarketDataDescription<T> marketDataDescription, out T curve) where T : class, IMarketDataSource;
    }
}