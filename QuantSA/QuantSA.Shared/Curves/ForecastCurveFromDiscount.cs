using System;
using QuantSA.Primitives.Dates;

namespace QuantSA.General
{
    /// <summary>
    /// Wraps a discount curve as a forward rate forecasting curve.
    /// </summary>
    [Serializable]
    public class ForecastCurveFromDiscount : IFloatingRateSource
    {
        private readonly IDiscountingSource discountCurve;
        private readonly IFloatingRateSource fixingCurve;
        private readonly FloatingIndex index;

        /// <summary>
        /// Will use the discount factors to obtain the forward rates after the curve's anchor date and the fixing curve before that date.
        /// </summary>
        /// <param name="discountCurve"></param>
        /// <param name="index"></param>
        /// <param name="fixingCurve"></param>
        public ForecastCurveFromDiscount(IDiscountingSource discountCurve, FloatingIndex index,
            IFloatingRateSource fixingCurve)
        {
            this.discountCurve = discountCurve;
            this.index = index;
            this.fixingCurve = fixingCurve;
        }

        public FloatingIndex GetFloatingIndex()
        {
            return index;
        }

        /// <summary>        
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public double GetForwardRate(Date date)
        {
            //TODO: Index should store the business day and daycount conventions of the index.            
            if (date > discountCurve.GetAnchorDate())
            {
                var df1 = discountCurve.GetDF(date);
                var laterDate = date.AddTenor(index.tenor);
                var df2 = discountCurve.GetDF(laterDate);
                var dt = (laterDate - date) / 365.0;
                var fwdRate = (df1 / df2 - 1) / dt;
                return fwdRate;
            }

            return fixingCurve.GetForwardRate(date);
        }
    }
}