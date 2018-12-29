using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.Core.MarketData
{
    /// <summary>
    /// Linearly interpolates forward rates.  Must only be used for a single floating index.  This is generally enforced 
    /// by the model using it.
    /// </summary>
    public class ForecastCurve : IFloatingRateSource
    {
        private readonly DatesAndRates _dateAndRates;
        private readonly FloatRateIndex _index;

        private ForecastCurve()
        {
        }

        /// <summary>
        /// Create a curve that linearly interpolates the provided forward rates.  Rates are not used to get discount factors.
        /// </summary>
        /// <remarks>
        /// If you want to obtain discount factors from the provided rates rather use: <see cref="DatesAndRates"/></remarks>
        /// <param name="anchorDate"></param>
        /// <param name="index"></param>
        /// <param name="dates"></param>
        /// <param name="rates"></param>
        /// <param name="maximumDate"></param>
        public ForecastCurve(Date anchorDate, FloatRateIndex index, Date[] dates, double[] rates,
            Date maximumDate = null)
        {
            _index = index;
            _dateAndRates = new DatesAndRates(index.Currency, anchorDate, dates, rates, maximumDate);
        }

        public FloatRateIndex GetFloatingIndex()
        {
            return _index;
        }

        double IFloatingRateSource.GetForwardRate(Date date)
        {
            return _dateAndRates.InterpAtDate(date);
        }
    }
}