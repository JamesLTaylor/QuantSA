using QSALite.Dates;

namespace QSALite.MarketData
{
    /// <summary>
    /// Common interface implemented by all market data sources.  These can be things such as expectations of
    /// tradable instruments like forward rates, specific model parameters or discount sources.
    /// </summary>
    public interface IMarketDataSource
    {
        Date GetAnchorDate();

        /// <summary>
        /// Only used to give sensible error messages.  When it makes sense consider getting the name from a
        /// <see cref="MarketDataDescription{T}.Name" /> to help users easily understand what type of curve this
        /// is.
        /// </summary>
        string GetName();
    }
}