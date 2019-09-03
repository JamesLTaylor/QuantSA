using QSALite.Dates;

namespace QSALite
{
    public interface IMarketObservableProvider
    {
        double GetValue(IMarketObservable observable, Date requiredDate);
    }
}