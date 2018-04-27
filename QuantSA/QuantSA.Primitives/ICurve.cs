using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.General.Dates;

namespace QuantSA.General
{
    /// <summary>
    /// A curve is simply a function between dates and values.  
    /// </summary>
    /// <remarks>
    /// If you want something more abstract than this rather just 
    /// interpolate across doubles.
    ///</remarks>
    public interface ICurve
    {
        double InterpAtDate(Date date);
    }

    /// <summary>
    /// Extension methods for the ICurve interface
    /// </summary>
    public static class ICurveExtensionMethods
    {
        /// <summary>
        /// Extend the ICurve to allow interpolation at an array of dates.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="dates"></param>
        /// <returns></returns>
        public static double[] InterpAtDates(this ICurve curve, Date[] dates)
        {
            return dates.Select(date => curve.InterpAtDate(date)).ToArray();
        }
    }
}
