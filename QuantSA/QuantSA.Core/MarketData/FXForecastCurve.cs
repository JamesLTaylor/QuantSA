using System;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.MarketData
{
    /// <summary>
    /// Currencies are quoted in units of counter currency per one unit of base currency.
    ///
    /// </summary>
    /// <example>If the ZAR USD exchange rate was 13.52 ZAR per USD then ZAR is the counter and USD the base.</example>
    public class FXForecastCurve : IFXSource
    {
        private readonly IDiscountingSource _baseCurrencyFXBasisCurve;
        private readonly IDiscountingSource _counterCurrencyFXBasisCurve;
        private readonly CurrencyPair _currencyPair;
        private readonly double _fxRateAtAnchorDate;

        private FXForecastCurve()
        {
        }

        /// <summary>
        /// Construct an FX source where the forwards are obtained from the discount factors on two basis curves.
        /// </summary>
        /// <param name="currencyPair"></param>
        /// <param name="fxRateAtAnchorDate"></param>
        /// <param name="baseCurrencyFXBasisCurve"></param>
        /// <param name="counterCurrencyFXBasisCurve"></param>
        public FXForecastCurve(CurrencyPair currencyPair, double fxRateAtAnchorDate,
            IDiscountingSource baseCurrencyFXBasisCurve,
            IDiscountingSource counterCurrencyFXBasisCurve)
        {
            _currencyPair = currencyPair ?? throw new ArgumentNullException(nameof(currencyPair));
            if (baseCurrencyFXBasisCurve.GetAnchorDate() != counterCurrencyFXBasisCurve.GetAnchorDate())
                throw new ArgumentException("The two basis curves must have the same anchor dates.");
            if (currencyPair.BaseCurrency != baseCurrencyFXBasisCurve.GetCurrency())
                throw new ArgumentException("The currency of the baseCurrencyFXBasisCurve must the base currency.");
            if (currencyPair.CounterCurrency != counterCurrencyFXBasisCurve.GetCurrency())
                throw new ArgumentException(
                    "The currency of the counterCurrencyFXBasisCurve must the counter currency.");
            _fxRateAtAnchorDate = fxRateAtAnchorDate;
            _baseCurrencyFXBasisCurve = baseCurrencyFXBasisCurve;
            _counterCurrencyFXBasisCurve = counterCurrencyFXBasisCurve;
        }

        public Currency GetBaseCurrency()
        {
            return _currencyPair.BaseCurrency;
        }

        public Currency GetCounterCurrency()
        {
            return _currencyPair.CounterCurrency;
        }

        public double GetRate(Date date)
        {
            return _fxRateAtAnchorDate * _baseCurrencyFXBasisCurve.GetDF(date) / _counterCurrencyFXBasisCurve.GetDF(date);
        }

        public CurrencyPair GetCurrencyPair()
        {
            return _currencyPair;
        }
    }
}