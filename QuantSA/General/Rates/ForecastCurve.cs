using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    /// <summary>
    /// Linearly interpolates forward rates.  Must only be used for a single floating index.  This is generally enforced 
    /// by the model using it.
    /// </summary>
    public class ForecastCurve : IFloatingRateSource
    {
        private DatesAndRates dateAndRates;
        private FloatingIndex index;

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
        public ForecastCurve(Date anchorDate, FloatingIndex index, Date[] dates, double[] rates, Date maximumDate = null)
        {
            this.index = index;
            dateAndRates = new DatesAndRates(anchorDate, dates, rates, maximumDate);
        }

        public FloatingIndex GetFloatingIndex()
        {
            return index;
        }

        double IFloatingRateSource.GetForwardRate(Date date)
        {
            return dateAndRates.InterpAtDate(date);            
        }
    }
}
