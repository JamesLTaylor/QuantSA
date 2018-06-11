using System;
using QuantSA.General;

namespace QuantSA.Shared.MarketObservables
{
    [Serializable]
    public class Share : MarketObservable
    {
        internal string shareCode;
        private readonly string toString;

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