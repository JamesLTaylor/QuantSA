using System.Linq;
using QuantSA.Primitives.Dates;

namespace QuantSA.General
{
    /// <summary>
    /// A curve is simply a function between dates and values.  
    /// </summary>
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
            return dates.Select(curve.InterpAtDate).ToArray();
        }
    }
}