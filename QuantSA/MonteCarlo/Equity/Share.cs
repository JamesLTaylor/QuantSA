using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonteCarlo.Equity
{
    public class Share : MarketObservable
    {
        private string shareCode;

        public Share(string shareCode)
        {
            this.shareCode = shareCode;
        }

        public override string ToString()
        {
            return "SHARE:PRICE:ZAR:" + shareCode;
        }
    }
}
