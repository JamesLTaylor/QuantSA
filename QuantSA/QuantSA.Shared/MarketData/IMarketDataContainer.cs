using QuantSA.General;
using QuantSA.Shared.MarketData;

namespace QuantSA.Primitives.MarketData
{
    /// <summary>
    /// A collection of <see cref="IMarketDataSource"/>s.
    /// </summary>
    internal interface IMarketDataContainer
    {
        IDiscountingSource GetDiscountingSource(IDiscountingDescription description);
    }
}