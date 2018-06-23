using QuantSA.Shared.Dates;

namespace QuantSA.Shared.MarketData
{
    /// <summary>
    /// Common interface implemented by all market data sources.
    /// </summary>
    public interface IMarketDataSource
    {
        Date AnchorDate { get; }
    }
}