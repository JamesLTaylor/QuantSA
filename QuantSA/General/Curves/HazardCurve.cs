using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;

namespace QuantSA.General
{
    /// <summary>
    /// Obtains survival probabilities from a piecewise linear interpolation in hazard rates.
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
        /// <param name="refEntity">The reference entity.</param>
        /// <param name="anchorDate">The anchor date.</param>
        /// <param name="dates">The dates on which the rates apply.</param>
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

        public override double GetSP(Date date)
        {
            if (date < anchorDate) throw new ArgumentException("Survival probabilities are only defined from the anchor date of the curve.");
            double rate = Tools.Interpolate1D(date.value, dateValues, hazardRates, hazardRates[0], hazardRates[hazardRates.Length() - 1]);
            return Math.Exp(-rate * (date - anchorDate)/365.0);
        }
    }
}
