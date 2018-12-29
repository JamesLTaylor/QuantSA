namespace QuantSA.Shared.MarketData
{
    /// <summary>
    /// A collection of <see cref="IMarketDataSource"/>s.
    /// </summary>
    public interface IMarketDataContainer
    {
        IDiscountingSource GetDiscountingSource(IDiscountingDescription description);
    }
}