using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Shared.Data
{
    /// <summary>
    /// Many objects in the library need to have single definitions that are shared everywhere.
    /// For example float rate indices and calendars.  All the data types that are mentioned  
    /// </summary>
    public interface ISharedInstances
    {
        bool MustUseSharedInstance(object instance);
        Currency GetCurrency(string code);
        CurrencyPair GetCurrencyPair(string name);
        FloatRateIndex GetFloatRateIndex(string name);
        Calendar GetCalendar(string name);
        void Add<T>(string name);
        T Get<T>(string name);
    }
}