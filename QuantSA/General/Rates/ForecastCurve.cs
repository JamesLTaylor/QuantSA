using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    /// <summary>
    /// Linearly interpolates forward rates.  Must only be used for a floating single index.  This is generally enforced 
    /// by the model using it.
    /// </summary>
    public class ForecastCurve : IFloatingRateSource
    {
        private DatesAndRates dateAndRates;

        public ForecastCurve(Date anchorDate, Date[] dates, double[] rates, Date maximumDate = null)
        {
            dateAndRates = new DatesAndRates(anchorDate, dates, rates, maximumDate);
        }

        double IFloatingRateSource.GetForwardRate(Date date)
        {
            return dateAndRates.InterpAtDate(date);            
        }
    }
}
