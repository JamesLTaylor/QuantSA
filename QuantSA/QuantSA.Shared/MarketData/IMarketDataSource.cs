using QuantSA.Shared.Dates;

namespace QuantSA.Shared.MarketData
{
    /// <summary>
    /// Common interface implemented by all market data sources.  These can be things such as expectations of
    /// tradable instruments like forward rates, specific model parameters and discount sources. 
    /// </summary>
    public interface IMarketDataSource
    {
        Date AnchorDate { get; }
    }
}