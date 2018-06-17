using System;

namespace QuantSA.General
{
    /// <summary>
    /// A currency pair.  Its observed value will always be units of counter currency 
    /// per single unit of base currency
    /// </summary>
    [Serializable]
    public class CurrencyPair : MarketObservable
    {
        private readonly string toString;

        /// <summary>
        /// Construct a market observable currency.  The value of this will be units of counter currency per single unit of base currency
        /// </summary>
        /// <remarks>As long as <see cref="Currency"/> is properly defined then so will this be.</remarks>
        /// <param name="baseCurrency"></param>
        /// <param name="counterCurrency"></param>
        public CurrencyPair(Currency baseCurrency, Currency counterCurrency)
        {
            this.baseCurrency = baseCurrency;
            this.counterCurrency = counterCurrency;
            toString = baseCurrency + ":" + counterCurrency;
        }

        public Currency baseCurrency { get; }
        public Currency counterCurrency { get; }

        public override string ToString()
        {
            return toString;
        }
    }
}