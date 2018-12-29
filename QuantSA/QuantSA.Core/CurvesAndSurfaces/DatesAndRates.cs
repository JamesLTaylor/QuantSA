using System;
using System.Linq;
using MathNet.Numerics.Interpolation;
using Newtonsoft.Json;
using QuantSA.Shared.CurvesAndSurfaces;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.CurvesAndSurfaces
{
    /// <summary>
    /// A collection of dates and rates for interpolating.  The rates can be used as continuously compounded rates to get 
    /// discount factors or interpolated directly.
    /// </summary>
    public class DatesAndRates : IDiscountingSource, ICurve
    {
        private readonly Date _anchorDate;

        //TODO: Separate this class into one that discounts and one that interpolates.  It could be abused/misused in its current form
        private readonly Currency _currency;
        private readonly Date[] _dates;
        private readonly Date _maximumDate;
        private readonly double[] _rates;

        [JsonIgnore] private LinearSpline _spline;

        private DatesAndRates()
        {
        }

        /// <summary>
        /// Creates a curve.  The curve will be flat from the anchor date to the first date and from the last date in dates until maximumDate
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="anchorDate">Date from which the curve applies.  Interpolation before this date won't work.</param>
        /// <param name="dates">Must be sorted in increasing order.</param>
        /// <param name="rates">The rates.  If the curve is going to be used to supply discount factors then these rates must be continuously compounded.</param>
        /// <param name="maximumDate">The date beyond which interpolation will not be allowed.  If it is null or left out then the last date in dates will be used.</param>
        public DatesAndRates(Currency currency, Date anchorDate, Date[] dates, double[] rates, Date maximumDate = null)
        {
            _currency = currency ?? throw new ArgumentNullException(nameof(currency));
            _anchorDate = anchorDate ?? throw new ArgumentNullException(nameof(anchorDate));
            _dates = dates ?? throw new ArgumentNullException(nameof(dates));
            _rates = rates ?? throw new ArgumentNullException(nameof(rates));
            _maximumDate = maximumDate;
        }

        [JsonIgnore]
        private LinearSpline Spline
        {
            get
            {
                if (_spline != null) return _spline;
                _spline = LinearSpline.InterpolateSorted(_dates.Select(d => (double) d.value).ToArray(), _rates);
                return _spline;
            }
        }

        /// <summary>
        /// Interpolate the curve.
        /// </summary>
        /// <param name="date">The date at which the rate is required.</param>
        /// <exception cref="ArgumentException">If <paramref name="date"/> is not in the range of the curve.</exception>
        /// <returns></returns>
        public double InterpAtDate(Date date)
        {
            var lastDate = _maximumDate ?? _dates.Last();
            if (date < _anchorDate)
                throw new ArgumentException($"Interpolation date  ({date}) is before the anchor date of the " +
                                            $"curve.({_anchorDate})");
            if (date > lastDate)
                throw new ArgumentException(
                    $"Interpolation date ({date}) is after the last date on the curve.({lastDate}");
            return Spline.Interpolate(date);
        }

        /// <summary>
        /// Get a discount factor assuming the rates are continuously compounded and the daycount in actual/365
        /// </summary>
        /// <param name="date">The date at which the discount factor (DF) is required.  The DF will apply from the anchor date to this date.</param>
        /// <returns></returns>
        public double GetDF(Date date)
        {
            var rate = InterpAtDate(date);
            var df = Math.Exp(-rate * (date - _anchorDate) / 365.0);
            return df;
        }

        public Currency GetCurrency()
        {
            return _currency;
        }

        [JsonIgnore] public Date AnchorDate => _anchorDate;
    }
}