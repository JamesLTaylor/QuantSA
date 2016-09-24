using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    public interface IFXSource
    {
        Currency GetBaseCurrency();
        Currency GetCounterCurrency();
        CurrencyPair GetCurrencyPair();
        double GetRate(Date date);
    }
}
