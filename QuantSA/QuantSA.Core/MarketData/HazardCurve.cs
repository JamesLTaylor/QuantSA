using System;
using Accord.Math;
using QuantSA.Core.Dates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.Core.MarketData
{
    /// <summary>
    /// Obtains survival probabilities from a piecewise linear interpolation in hazard rates.
    /// <para/>
    /// The curve parameterizes lambda as a function of T, the time in years since the anchor date and 
    /// survival is given by exp(-lambda(T)*T)
    /// </summary>
    /// <seealso cref="SurvivalProbabilitySource" />
    public class HazardCurve : SurvivalProbabilitySource
    {
        private readonly double[] _hazardRates;
        private Date[] _dates;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazardCurve"/> class.
        /// </summary>
        /// <param name="referenceEntity">The reference entity for whom these hazard rates apply.</param>
        /// <param name="anchorDate">The anchor date.  Survival probabilities can only be calculated up to dates after this date.</param>
        /// <param name="dates">The dates on which the hazard rates apply.</param>
        /// <param name="hazardRates">The hazard rates.</param>
        /// <exception cref="System.ArgumentException">
        /// dates must be on or after the anchor date.
        /// dates must be increasing
        /// dates and rates must have the same length.
        /// </exception>
        public HazardCurve(ReferenceEntity referenceEntity, Date anchorDate, Date[] dates, double[] hazardRates) :
            base(referenceEntity, anchorDate)
        {
            _dates = dates;
            _hazardRates = hazardRates;
        }

        /// <summary>
        /// Gets the survival probability between the anchor date and the date provided.
        /// </summary>
        /// <param name="date">The date up to which the survival probability will be calculated.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Survival probabilities are only defined from the anchor date of the curve.</exception>
        public override double GetSP(Date date)
        {
            if (date < anchorDate)
                throw new ArgumentException(
                    "Survival probabilities are only defined from the anchor date of the curve.");
            var rate = Tools.Interpolate1D(date.value, _dates.GetValues(), _hazardRates, _hazardRates[0],
                _hazardRates[_hazardRates.Length - 1]);
            return Math.Exp(-rate * (date - anchorDate) / 365.0);
        }
    }
}