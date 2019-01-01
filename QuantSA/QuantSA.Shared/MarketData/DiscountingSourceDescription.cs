using Newtonsoft.Json;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Shared.MarketData
{
    /// <summary>
    /// A description of an <see cref="IDiscountingSource" /> and what it should be used for.  For example
    /// the source might specify that it can be used for any discounting in a particular <see cref="Currency" />
    /// or perhaps only discounting in a cashflows in currency when the trade has a particular collateral type or rule.
    /// </summary>
    public class DiscountingSourceDescription : MarketDataDescription<IDiscountingSource>
    {
        private readonly MarketObservable _collateralType;
        private readonly Currency _currency;
        private readonly FloatRateIndex _iborIndex;
        private string _name;

        /// <summary>
        /// A discount curve for a single currency. Only use this when there is a single discounting curve in the currency.
        /// </summary>
        /// <param name="currency"></param>
        public DiscountingSourceDescription(Currency currency)
        {
            _currency = currency;
        }

        /// <summary>
        /// An Ibor curve that can be used for discounting.
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="iborIndex"></param>
        public DiscountingSourceDescription(Currency currency, FloatRateIndex iborIndex)
        {
            _currency = currency;
            _iborIndex = iborIndex;
        }

        /// <summary>
        /// A discounting curve for a type of collateral.
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="collateralType"></param>
        public DiscountingSourceDescription(Currency currency, MarketObservable collateralType)
        {
            _currency = currency;
            _collateralType = collateralType;
        }

        public override string Name
        {
            get
            {
                if (_name != null)
                    return _name;
                if (_iborIndex != null)
                    _name = $"DiscountingSource.{_currency}.{_iborIndex}";
                else if (_collateralType != null)
                    _name = $"DiscountingSource.{_currency}.{_collateralType}";
                else
                    _name = $"DiscountingSource.{_currency}";
                return _name;
            }
        }

        [JsonIgnore] public Currency Currency => _currency;
    }
}