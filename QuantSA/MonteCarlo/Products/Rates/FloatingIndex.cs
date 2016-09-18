using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA;

namespace MonteCarlo
{
    public class FloatingIndex : MarketObservable
    {
        private Currency currency;
        private string name;
        private Tenor tenor;

        public FloatingIndex(Currency currency, string name, Tenor tenor)
        {
            this.currency = currency;
            this.name = name;
            this.tenor = tenor;
        }

        public override string ToString()
        {
            return currency.ToString() + ":" + name.ToUpper() + ":" + tenor.ToString();
           
        }

        #region Stored Indices
        public static FloatingIndex JIBAR3M = new FloatingIndex(Currency.ZAR, "Jibar", Tenor.Months(3));
        #endregion
    }
}
