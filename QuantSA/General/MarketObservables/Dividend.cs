namespace QuantSA.General
{
    public class Dividend : MarketObservable
    {
        public Share underlying { get; private set; }
        private string toString;

        public Dividend(Share underlying)
        {
            this.underlying = underlying;
            toString = "SHARE:DIVI:" + underlying.currency.ToString() + ":" + underlying.shareCode;
        }
        
        public override string ToString()
        {
            return toString;
        }
    }
}