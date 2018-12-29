using QuantSA.Shared.Primitives;

namespace QuantSA.Shared.MarketData
{
    /// <summary>
    /// A description of an <see cref="IDiscountingSource"/> and what it should be used for.  For example
    /// the source might specify that it can be used for any discounting in a particular <see cref="Currency"/>
    /// or perhaps only discounting in a cashflows in currency when the trade has a particular collateral type or rule.
    /// </summary>
    public class DiscountingSourceDescription : MarketDataDescription<IDiscountingSource>
    {
        private readonly Currency _currency;
        private string _name;

        public override string Name
        {
            get
            {
                if (_name != null)
                    return _name;
                _name = $"IDiscountingSource:{_currency}";
                return _name;
            }
        }

        public DiscountingSourceDescription(Currency currency)
        {
            _currency = currency;
        }
    }
}