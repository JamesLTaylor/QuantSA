using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// A currency pair.  Its observed value will always be units units of counter currency 
    /// per single unit of base currency
    /// </summary>
    [Serializable]    
    public class CurrencyPair : MarketObservable
    {
        public Currency baseCurrency { get; private set; }
        public Currency counterCurrency { get; private set; }
        string toString;

        /// <summary>
        /// Construct a market obserable currency.  The value of this will be units of counter currency per single unit of base currency
        /// </summary>
        /// <remarks>As long as <see cref="Currency"/> is properly defined then so will this be.</remarks>
        /// <param name="baseCurrency"></param>
        /// <param name="counterCurrency"></param>
        public CurrencyPair(Currency baseCurrency, Currency counterCurrency)
        {
            this.baseCurrency = baseCurrency;
            this.counterCurrency = counterCurrency;
            toString = baseCurrency.ToString() + ":" + counterCurrency.ToString();
        }

        public override string ToString()
        {
            return toString;
        }
    }
}
