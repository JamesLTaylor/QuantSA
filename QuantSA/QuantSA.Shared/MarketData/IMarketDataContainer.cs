using QuantSA.Shared.Dates;

namespace QuantSA.Shared.MarketData
{
    /// <summary>
    /// A collection of <see cref="IMarketDataSource"/>s.
    /// </summary>
    public interface IMarketDataContainer
    {
        bool Contains<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource;

        T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource;

        void Set(IMarketDataSource curve);

        void Calibrate(Date calibrationDate);
    }
}