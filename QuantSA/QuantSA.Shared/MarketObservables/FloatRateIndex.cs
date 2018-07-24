using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Shared.MarketObservables
{
    /// <summary>
    /// An object to describe a floating rate index such as 3 Month Jibar.
    /// </summary>
    public class FloatRateIndex : MarketObservable
    {
        public Currency Currency { get; }
        public Tenor Tenor { get; }
        private readonly string _indexName;
        private readonly string _objectName;

        public FloatRateIndex(string objectName, Currency currency, string indexName, Tenor tenor)
        {
            Currency = currency;
            _indexName = indexName;
            Tenor = tenor;
            _objectName = objectName;
        }

        public override string ToString()
        {
            return _objectName;
        }
    }
}