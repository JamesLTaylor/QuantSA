using QSALite.Dates;

namespace QSALite
{
    public struct Cashflow
    {
        public readonly Date Date;
        public readonly double Amount;
        public readonly Currency Currency;

        public Cashflow(Date date, double amount, Currency currency)
        {
            Date = date;
            Amount = amount;
            Currency = currency;
        }
    }
}