using QuantSA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarlo
{
    public class Share : MarketObservable
    {
        private string shareCode;
        public Currency currency { get; private set; }
        
        public Share(string shareCode, Currency currency)
        {
            this.shareCode = shareCode;
            this.currency = currency;
        }
        
        public override string ToString()
        {
            return "SHARE:PRICE:" + currency.ToString() + ":" + shareCode;
        }
    }
}
