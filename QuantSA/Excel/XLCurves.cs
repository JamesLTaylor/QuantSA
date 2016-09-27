using ExcelDna.Integration;
using QuantSA;
using System;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public static class XLCurves
    {
        [QuantSAExcelFunction(Description = "Create a best fit Nelson Siegel curve.  Can be used anywhere as a curve. (Curve)",
        Name = "QSA.FitCurveNelsonSiegel",
        Category = "QSA.Curves",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/FitCurveNelsonSiegel.html")]
        public static object FitCurveNelsonSiegel([ExcelArgument(Description = "Name of fitted curve.")]String name,
                [ExcelArgument(Description = "The date at which the resultant curve will be anchored.  Can be set to zero.")]object[,] anchorDate,
                [ExcelArgument(Description = "The dates at which rates apply.")]object[,] dates,
                [ExcelArgument(Description = "The rates to be fitted")]Double[] rates)
        {
            try {
                NelsonSiegel curve = NelsonSiegel.Fit(XU.GetDates0D(anchorDate, "anchorDate"), XU.GetDates1D(dates, "dates"), rates);
                return XU.AddObject(name, curve);
            } catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "Find the interpolated value of any QuantSA created curve.",
        Name = "QSA.CurveInterp",
        Category = "QSA.Curves",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/CurveInterp.html")]
        public static object[,] CurveInterp([ExcelArgument(Description = "The name of the curve to interpolate. (Curve)")]String name,
        [ExcelArgument(Description = "The dates at which interpolated rates are required.")]object[,] dates)
        {
            try {
                ICurve curve = ObjectMap.Instance.GetObjectFromID<ICurve>(name);
                Date[,] dtDates = XU.GetDates2D(dates,"dates");
                object[,] result = new object[dtDates.GetLength(0), dtDates.GetLength(1)];

                for (int row = 0; row < dtDates.GetLength(0); row += 1)
                {
                    for (int col = 0; col < dtDates.GetLength(1); col += 1)
                    {
                        result[row, col] = curve.InterpAtDate(dtDates[row, col]);
                    }
                }
                return result;
            } catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }


        [QuantSAExcelFunction(Description = "Create a curve simulator based on principle components.",
        Name = "QSA.CreatePCACurveSimulator",
        Category = "QSA.Curves",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/CreateDatesAndRatesCurve.html")]
        public static object CreatePCACurveSimulator([ExcelArgument(Description = "The name of the simulator")]string simulatorName,
            [ExcelArgument(Description = "The date from which the curve dates will be calculated.")]object[,] anchorDate,
            [ExcelArgument(Description = "The starting rates.  Must be the same length as the elements in the component vectors.")]double[] initialRates,
            [ExcelArgument(Description = "The months at which each rate applies.")]double[] tenorMonths,
            [ExcelArgument(Description = "The componenents.  Each component in a column.  Stack the columns side by side.")]double[,] components,
            [ExcelArgument(Description = "The volatility for each component.  Must be the same length as the number of components.")]double[] vols,
            [ExcelArgument(Description = "All rates will be multiplied by this amount.  This should almost always be 1.0.")]double multiplier,
            [ExcelArgument(Description = "Indicates if the PCA was done on relative moves.  If not then it was done on absolute moves.")]bool useRelative)
        {
            try
            {
                int[] tenorMonthsInt = new int[tenorMonths.Length];
                for (int i = 0; i < tenorMonths.Length; i++) { tenorMonthsInt[i] = (int)tenorMonths[i]; }
                PCACurveSimulator curveSimulator = new PCACurveSimulator(XU.GetDates0D(anchorDate, "anchorDate"), 
                    initialRates, tenorMonthsInt, components, vols, multiplier, XU.GetBool(useRelative));
                return ObjectMap.Instance.AddObject(simulatorName, curveSimulator);                
            }
            catch (Exception e)
            {
                return XU.Error2D(e);                
            }
        }


        [QuantSAExcelFunction(Description = "Gets a block of principle component simulated rates.  Each row is a curve at a simulation date.",
        Name = "QSA.PCACurveSimulatorGetRates",
        Category = "QSA.Curves",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static object[,] PCACurveSimulatorGetRates([ExcelArgument(Description = "The name of the simulator. (PCACurveSimulator)")]string simulatorName,
            [ExcelArgument(Description = "A list of increasing dates.")]object[,] simulationDates,
            [ExcelArgument(Description = "The tenors at which the rates are required.  Does not need to be the same as used to do the PCA.")]double[] requiredTenorMonths)
        {
            try
            {
                int[] tenorMonthsInt = new int[requiredTenorMonths.Length];
                for (int i = 0; i < requiredTenorMonths.Length; i++) { tenorMonthsInt[i] = (int)requiredTenorMonths[i]; }


                PCACurveSimulator curveSimulator = ObjectMap.Instance.GetObjectFromID<PCACurveSimulator>(simulatorName);
                double[,] result = curveSimulator.GetSimulatedRates(XU.GetDates1D(simulationDates, "simulationDates"), tenorMonthsInt);
                return XU.ConvertToObjects(result);
            }

            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }


        [QuantSAExcelFunction(Description = "Create a curve of dates and rates.",
        Name = "QSA.CreateDatesAndRatesCurve",
        Category = "QSA.Curves",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/CreateDatesAndRatesCurve.html")]
        public static object CreateDatesAndRatesCurve([ExcelArgument(Description = "The name of the curve.")]string name,
            [ExcelArgument(Description = "The dates at which the rates apply.")]object[,] dates,
            [ExcelArgument(Description = "The rates.")]double[] rates,
            [ExcelArgument(Description = "Optional: The currency that this curve can be used for discounting.  Leave blank to use for any currency. (Currency)")]object[,] currency)
        {
            try
            {
                var dDates = XU.GetDates1D(dates, "dates");
                DatesAndRates curve = new DatesAndRates(XU.GetCurrencies0D(currency, "currency", true), dDates[0], dDates, rates);
                return ObjectMap.Instance.AddObject(name, curve);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }



        [QuantSAExcelFunction(Description = "Get the covariance in log returns from a blob of curves.",
        Name = "QSA.CovarianceFromCurves",
        Category = "QSA.Curves",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/CovarianceFromCurves.html")]
        public static double[,] CovarianceFromCurves([ExcelArgument(Description = "Blob of curves, each row is a curve of the same length.")]double[,] curves)
        {
            double[,] covMatrix = General.DataAnalysis.PCA.CovarianceFromCurves(curves);
            return covMatrix;
        }

        [QuantSAExcelFunction(Description = "Perform a PCA on the log returns of a blob of curves.",
        Name = "QSA.PCAFromCurves",
        Category = "QSA.Curves",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/PCAFromCurves.html")]
        public static object[,] PCAFromCurves([ExcelArgument(Description = "Blob of curves, each row is a curve of the same length.")]double[,] curves,
            [ExcelArgument(Description = "Indicates if the PCA is to be done on relative moves.  If not then it will be done on absolute moves.")]object useRelative)
        {
            try {
                double[,] covMatrix = QuantSA.General.DataAnalysis.PCA.PCAFromCurves(curves, XU.GetBool(useRelative));
                return XU.ConvertToObjects(covMatrix);
            }
            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }
    }
}
