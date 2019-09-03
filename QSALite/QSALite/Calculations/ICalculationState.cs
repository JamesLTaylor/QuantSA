using QSALite.Dates;
using QSALite.MarketData;
using QSALite.Products;

namespace QSALite.Calculations
{
    public interface ICalculationState
    {
        IMarketDataContainer MarketData { get; }
        ICalculation Calculation { get; }
        Date ValueDate { get; }
        Portfolio Portfolio { get; }
    }
}