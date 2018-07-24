using QuantSA.Shared.Primitives;

namespace QuantSA.Shared.MarketObservables
{
    /// <summary>
    /// A currency pair.  Its observed value will always be units of counter currency 
    /// per single unit of base currency
    /// </summary>
    public class CurrencyPair : MarketObservable
    {
        private readonly string _name;

        /// <summary>
        /// Construct a market observable currency.  The value of this will be units of counter currency per single unit of base currency
        /// </summary>
        /// <remarks>As long as <see cref="Currency"/> is properly defined then so will this be.</remarks>
        /// <param name="name"></param>
        /// <param name="baseCurrency"></param>
        /// <param name="counterCurrency"></param>
        public CurrencyPair(string name, Currency baseCurrency, Currency counterCurrency)
        {
            _name = name;
            BaseCurrency = baseCurrency;
            CounterCurrency = counterCurrency;
        }

        public Currency BaseCurrency { get; }
        public Currency CounterCurrency { get; }

        public override string ToString()
        {
            return _name;
        }
    }
}