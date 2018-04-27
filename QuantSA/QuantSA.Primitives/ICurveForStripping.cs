using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.Dates;

namespace QuantSA.General
{
    public interface ICurveForStripping
    {
        /// <summary>
        /// Sets the dates on which the curve will have values.  After this is set it must
        /// be possible to call <see cref="GetRates"/> and get an array the same length as 
        /// 
        /// </summary>
        /// <param name="dates">The dates.</param>
        void SetDates(Date[] dates);
        double[] GetRates();
        void SetRate(int index, double rate);        
        //double GetSmoothness();
    }
}
