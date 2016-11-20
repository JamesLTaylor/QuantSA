using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// Wraps a discount curve as a forward rate forecasting curve.
    /// </summary>
    [Serializable]
    public class ForecastCurveFromDiscount : IFloatingRateSource
    {
        IDiscountingSource discountCurve;
        FloatingIndex index;
        IFloatingRateSource fixingCurve;

        /// <summary>
        /// Will use the discount factors to obtain the forward rates after the curve's anchor date and the fixing curve before that date.
        /// </summary>
        /// <param name="discountCurve"></param>
        /// <param name="index"></param>
        /// <param name="fixingCurve"></param>
        public ForecastCurveFromDiscount(IDiscountingSource discountCurve, FloatingIndex index, IFloatingRateSource fixingCurve)
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
            if (date > discountCurve.getAnchorDate())
            {
                double df1 = discountCurve.GetDF(date);
                Date laterDate = date.AddTenor(index.tenor);
                double df2 = discountCurve.GetDF(laterDate);
                double dt = (laterDate - date) / 365.0;
                double fwdRate = (df1 / df2 - 1) / dt;
                return fwdRate;
            }
            else
            {
                return fixingCurve.GetForwardRate(date);
            }            
        }
    }
}
