using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Shared.MarketData
{
    public interface IDiscountingSource : IMarketDataSource
    {
        /// <summary>
        /// The currency of cashflows for which this curve should be used for discounting.
        /// </summary>
        /// <returns></returns>
        Currency GetCurrency();

        /// <summary>
        /// The discount factor from the curves natural anchor date to the provided date.
        /// </summary>
        /// <param name="date">Date on which the discount factor is required.</param>
        /// <returns></returns>
        double GetDF(Date date);
    }
}