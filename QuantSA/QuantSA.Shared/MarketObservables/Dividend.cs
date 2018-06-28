using System;

namespace QuantSA.Shared.MarketObservables
{
    
    public class Dividend : MarketObservable
    {
        private readonly string toString;

        public Dividend(Share underlying)
        {
            this.underlying = underlying;
            toString = "SHARE:DIVI:" + underlying.currency + ":" + underlying.shareCode;
        }

        public Share underlying { get; }

        public override string ToString()
        {
            return toString;
        }
    }
}