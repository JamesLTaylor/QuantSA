using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantSA.Shared.CurvesAndSurfaces;
using QuantSA.Shared.Dates;

namespace QuantSA.Core.CurvesAndSurfaces
{
    /// <summary>
    /// Extension methods for the ICurve interface
    /// </summary>
    public static class CurveEx
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
