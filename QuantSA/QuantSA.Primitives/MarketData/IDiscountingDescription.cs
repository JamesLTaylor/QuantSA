using QuantSA.General;

namespace QuantSA.Primitives.MarketData
{
    /// <summary>
    /// A description of an <see cref="IDiscountingSource"/> and what it should be used for.  For example
    /// the source might specify that it can be used for any discounting in a particular <see cref="Currency"/>
    /// or perhaps only discounting in a cashflows in currency when the trade has a particular collateral type or rule.
    /// </summary>
    public interface IDiscountingDescription
    {
        string GetName();
    }
}