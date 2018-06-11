using QuantSA.Primitives.Dates;

namespace QuantSA.Primitives.MarketData
{
    /// <summary>
    /// Common interface implemented by all market data sources.
    /// </summary>
    public interface IMarketDataSource
    {
        Date AnchorDate { get; }
    }
}