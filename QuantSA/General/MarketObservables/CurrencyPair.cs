using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    public class CurrencyPair : MarketObservable
    {
        Currency baseCurrency;
        Currency counterCurrency;
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
