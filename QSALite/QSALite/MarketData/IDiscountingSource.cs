using QSALite.Dates;

namespace QSALite.MarketData
{
    public interface IDiscountingSource : IMarketDataSource
    {
        double GetDF(Date date);
    }
}