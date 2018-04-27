using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using QuantSA.General.Dates;

namespace QuantSA.General
{
    /// <summary>
    /// Obtains survival probabilities from a piecewise linear interpolation in hazard rates.
    /// <para/>
    /// The curve parameterizes lambda as a function of T, the time in years since the anchor date and 
    /// survival is given by exp(-lambda(T)*T)
    /// </summary>
    /// <seealso cref="QuantSA.General.ISurvivalProbabilitySource" />
    [Serializable]
    public class HazardCurve : ISurvivalProbabilitySource
    {
        double[] dateValues;
        double[] hazardRates;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazardCurve"/> class.
        /// </summary>
        /// <param name="refEntity">The reference entity for whom these hazard rates apply.</param>
        /// <param name="anchorDate">The anchor date.  Survival probabilites can only be calculated up to dates after this date.</param>
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
            if (dates[0] < anchorDate) throw new ArgumentException("dates must be on or after the anchor date.");
            for (int i  = 0; i < dates.Length - 1; i++)
            {
                if (dates[i] > dates[i + 1]) throw new ArgumentException("dates must be increasing.");                
            }
            dateValues = dates.GetValues();
            if (dateValues.Length != hazardRates.Length) throw new ArgumentException("dates and rates must have the same length.");
            this.hazardRates = hazardRates;
        }

        /// <summary>
        /// Gets the survival probability between the anchor date and the date provided.
        /// </summary>
        /// <param name="date">The date up to which the survival probability will be calculated.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Survival probabilities are only defined from the anchor date of the curve.</exception>
        public override double GetSP(Date date)
        {
            if (date < anchorDate) throw new ArgumentException("Survival probabilities are only defined from the anchor date of the curve.");
            double rate = Tools.Interpolate1D(date.value, dateValues, hazardRates, hazardRates[0], hazardRates[hazardRates.Length() - 1]);
            return Math.Exp(-rate * (date - anchorDate)/365.0);
        }
    }
}
