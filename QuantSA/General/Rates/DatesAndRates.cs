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
        
        public DatesAndRates(double[] dates, double[] rates)
        {
            this.dates = dates;
            this.rates = rates;            
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
        /// Interpolate the curve.  Be careful to use the same time basis as the curve was constructed with.
        /// </summary>
        /// <param name="time">The time at which the rate is required.</param>
        /// <returns></returns>
        public double InterpAtTime(double time)
        {
            LinearSpline spline = LinearSpline.InterpolateSorted(dates, rates);
            return spline.Interpolate(time);            
        }
    }
}
