using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.General.Dates;

namespace QuantSA.General
{
    /// <summary>
    /// Currencies are quoted in units of counter currency per one unit of base currency.
    ///
    /// </summary>
    /// <example>If the ZAR USD exchnage rate was 13.52 ZAR per USD then ZAR is the counter and USD the base.</example>
    [Serializable]
    public class FXForecastCurve : IFXSource
    {
        Currency baseCurrency;
        Currency counterCurrency;
        CurrencyPair currencyPair;
        double fxRateAtAnchorDate;
        IDiscountingSource baseCurrencyFXBasisCurve;
        IDiscountingSource counterCurrencyFXBasisCurve;

        /// <summary>
        /// Construct an FX source where the forwards are obtained from the discount factors on two basis curves.
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <param name="counterCurrency"></param>
        /// <param name="fxRateAtAnchorDate"></param>
        /// <param name="baseCurrencyFXBasisCurve"></param>
        /// <param name="counterCurrencyFXBasisCurve"></param>
        public FXForecastCurve(Currency baseCurrency, Currency counterCurrency, double fxRateAtAnchorDate, IDiscountingSource baseCurrencyFXBasisCurve, 
            IDiscountingSource counterCurrencyFXBasisCurve)
        {
            if (baseCurrencyFXBasisCurve.GetAnchorDate() != counterCurrencyFXBasisCurve.GetAnchorDate()) throw new ArgumentException("The two basis curves must have the same anchor dates.");
            if (baseCurrency != baseCurrencyFXBasisCurve.GetCurrency()) throw new ArgumentException("The currency of the baseCurrencyFXBasisCurve must the base currency.");
            if (counterCurrency != counterCurrencyFXBasisCurve.GetCurrency()) throw new ArgumentException("The currency of the counterCurrencyFXBasisCurve must the counter currency.");
            this.baseCurrency = baseCurrency;
            this.counterCurrency = counterCurrency;
            this.fxRateAtAnchorDate = fxRateAtAnchorDate;
            this.baseCurrencyFXBasisCurve = baseCurrencyFXBasisCurve;
            this.counterCurrencyFXBasisCurve = counterCurrencyFXBasisCurve;
            currencyPair = new CurrencyPair(baseCurrency, counterCurrency);

        }
        public Currency GetBaseCurrency()
        {
            return baseCurrency;
        }

        public Currency GetCounterCurrency()
        {
            return counterCurrency;
        }

        public double GetRate(Date date)
        {
            return fxRateAtAnchorDate * baseCurrencyFXBasisCurve.GetDF(date) / counterCurrencyFXBasisCurve.GetDF(date);
        }

        public CurrencyPair GetCurrencyPair()
        {
            return currencyPair;
        }
    }
}
