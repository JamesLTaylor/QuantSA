using MathNet.Numerics.Interpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    [Serializable]
    public class DatesAndRates : IFloatingRateSource, IDiscountingSource, ICurve
    {
        private double[] dates;
        private double[] rates;
        
        public DatesAndRates(Date[] dates, double[] rates)
        {
            this.dates = dates.GetValues();
            this.rates = rates.Clone() as double[];            
        }

        public double GetDF(Date date)
        {
            throw new NotImplementedException();
        }

        public double GetForwardRate(Date date)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Interpolate the curve.
        /// </summary>
        /// <param name="time">The time at which the rate is required.</param>
        /// <returns></returns>
        public double InterpAtDate(Date date)
        {
            LinearSpline spline = LinearSpline.InterpolateSorted(dates, rates);
            return spline.Interpolate(date);            
        }
    }
}
