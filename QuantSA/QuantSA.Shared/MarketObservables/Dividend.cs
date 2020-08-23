namespace QuantSA.Shared.MarketObservables
{
    
    public class Dividend : MarketObservable
    {
        private readonly string _toString;

        public Dividend(Share underlying)
        {
            this.Underlying = underlying;
            _toString = "SHARE:DIVI:" + underlying.Currency + ":" + underlying.ShareCode;
        }

        public Share Underlying { get; }

        public override string ToString()
        {
            return _toString;
        }
    }
}