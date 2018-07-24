using QuantSA.Shared.Primitives;

namespace QuantSA.Shared.MarketObservables
{
    public class Share : MarketObservable
    {
        private readonly string toString;
        internal string shareCode;

        public Share(string shareCode, Currency currency)
        {
            this.shareCode = shareCode;
            this.currency = currency;
            toString = "SHARE:PRICE:" + currency + ":" + shareCode;
        }

        public Currency currency { get; }

        public override string ToString()
        {
            return toString;
        }
    }
}