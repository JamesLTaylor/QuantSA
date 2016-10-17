
namespace QuantSA.General
{
    public class Share : MarketObservable
    {
        internal string shareCode;
        public Currency currency { get; private set; }
        private string toString;
        
        public Share(string shareCode, Currency currency)
        {
            this.shareCode = shareCode;
            this.currency = currency;
            toString = "SHARE:PRICE:" + currency.ToString() + ":" + shareCode;
        }
        
        public override string ToString()
        {
            return toString;
        }
    }
}
