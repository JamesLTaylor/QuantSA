using QSALite.Dates;

namespace QSALite.MarketObservables
{
    public class FloatRateIndex : IMarketObservable
    {
        public readonly Currency Currency;
        public readonly string Name;
        public readonly Tenor Tenor;
    }
}