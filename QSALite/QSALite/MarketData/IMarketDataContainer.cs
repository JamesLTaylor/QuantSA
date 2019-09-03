namespace QSALite.MarketData
{
    public interface IMarketDataContainer
    {
        T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource;
    }
}