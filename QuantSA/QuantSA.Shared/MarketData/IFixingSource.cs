using QuantSA.General;
using QuantSA.Shared.Dates;

namespace QuantSA.Shared.MarketData
{
    /// <summary>
    /// A fixing source for observables that have already fixed before the valuation date.
    /// </summary>
    public interface IFixingSource
    {
        double Get(MarketObservable index, Date date);
    }
}