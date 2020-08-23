using QuantSA.Shared.Primitives;

namespace QuantSA.Shared.MarketObservables
{
    public class Share : MarketObservable
    {
        private readonly string _toString;
        internal readonly string ShareCode;

        public Share(string shareCode, Currency currency)
        {
            ShareCode = shareCode;
            Currency = currency;
            _toString = "SHARE:" + currency + ":" + shareCode;
        }

        public Currency Currency { get; }

        public override string ToString()
        {
            return _toString;
        }
    }
}