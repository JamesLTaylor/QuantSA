using System;
using QuantSA.General;
using QuantSA.Shared.Dates;

namespace QuantSA.Shared.Primitives
{
    [Serializable]
    public class Cashflow
    {
        public Cashflow(Date date, double amount, Currency currency)
        {
            Date = date;
            Amount = amount;
            Currency = currency;
        }

        public double Amount { get; }
        public Currency Currency { get; }
        public Date Date { get; }
    }
}