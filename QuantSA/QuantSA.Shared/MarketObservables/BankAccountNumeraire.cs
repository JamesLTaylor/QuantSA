using QuantSA.Shared.Primitives;

namespace QuantSA.Shared.MarketObservables
{
    public class BankAccountNumeraire : MarketObservable
    {
        private Currency _ccy;

        public BankAccountNumeraire(Currency ccy)
        {
            _ccy = ccy;
        }

        public override string ToString()
        {
            return $"BankAccount.{_ccy}";
        }
    }
}