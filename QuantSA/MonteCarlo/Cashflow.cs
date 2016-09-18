using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA;

namespace MonteCarlo
{
    public class Cashflow
    {
        public double amount { get; private set; }
        public Currency currency { get; private set; }
        public Date date { get; private set; }

        public Cashflow(Date date, double amount, Currency currency)
        {
            this.date = date;
            this.amount = amount;
            this.currency = currency;
        }

    }
}
