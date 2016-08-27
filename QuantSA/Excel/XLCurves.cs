using Curves;
using ExcelDna.Integration;
using System;

namespace Excel
{
    public static class XLCurves
    {
        [QuantSAExcelFunction(Description = "Create a best fit Nelson Siegel curve.  Can be used anywhere as a curve.",
        Name = "QSA.FitCurveNelsonSiegel",
        Category = "QSA.General",
        HelpTopic = "https://www.google.co.za")]
        public static string FitCurveNelsonSiegel([ExcelArgument(Description = "Name of object")]String name,
                [ExcelArgument(Description = "Times at which rates apply.")]double[] times,
                [ExcelArgument(Description = "Rates to be fitted")]Double[] rates)
        {
            NelsonSiegel curve = NelsonSiegel.Fit(times, rates);
            return ObjectMap.Instance.AddObject(name, curve);
        }

        [QuantSAExcelFunction(Description = "Find the interpolated value of any QuantSA created curve",
        Name = "QSA.CurveInterp",
        Category = "QSA.General",
        HelpTopic = "https://www.google.co.za")]
        public static object[,] CurveInterp([ExcelArgument(Description = "The name of the curve to interpolate")]String name,
        [ExcelArgument(Description = "The times at which interpolated rates are required.")]double[,] times)
        {
            NelsonSiegel curve = (NelsonSiegel)ObjectMap.Instance.GetObjectFromID(name);
            object[,] result = new object[times.GetLength(0), times.GetLength(1)];

            for (int row = 0; row < times.GetLength(0); row += 1)
            {
                for (int col = 0; col < times.GetLength(1); col += 1)
                {
                    result[row, col] = curve.Interp(new double[] { times[row, col] })[0];
                }
            }
            return result;
        }


    }
}
