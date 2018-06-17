using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.Shared.MarketData
{
    public interface IFloatingRateSource
    {
        /// <summary>
        /// The forward rate that applies on <paramref name="date"/>.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        double GetForwardRate(Date date);

        /// <summary>
        /// The <see cref="FloatRateIndex"/> that this curve forecasts.
        /// </summary>
        /// <returns></returns>
        FloatRateIndex GetFloatingIndex();
    }
}