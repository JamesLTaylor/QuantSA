using System.Collections.Generic;
using QSALite.Dates;

namespace QSALite.Products
{
    public interface IProduct
    {
        List<Cashflow> GetCashflows(IMarketObservableProvider marketObservableProvider);
        IProduct CopyWithNewValueDate(Date newValueDate);
    }
}