using ExcelDna.Integration;
using QuantSA.General;
using QuantSA.Excel;

namespace QuantSA.ExcelFunctions
{
    public static class XLCurves
    {
        [QuantSAExcelFunction(Description = "Create a best fit Nelson Siegel curve.  Can be used anywhere as a curve. (Curve)",
            Name = "QSA.FitCurveNelsonSiegel",
            HasGeneratedVersion = true,
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/FitCurveNelsonSiegel.html")]
        public static ICurve FitCurveNelsonSiegel([ExcelArgument(Description = "The date at which the resultant curve will be anchored.  Can be set to zero.")]Date anchorDate,
                [ExcelArgument(Description = "The dates at which rates apply.")]Date[] dates,
                [ExcelArgument(Description = "The rates to be fitted")]double[] rates)
        {
            return NelsonSiegel.Fit(anchorDate, dates, rates);
        }


        [QuantSAExcelFunction(Description = "Find the interpolated value of any QuantSA created curve.",
            Name = "QSA.CurveInterp",
            HasGeneratedVersion = true,
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CurveInterp.html")]
        public static double[,] CurveInterp([ExcelArgument(Description = "The curve to interpolate.")]ICurve curve,
        [ExcelArgument(Description = "The dates at which interpolated rates are required.")]Date[,] dates)
        {
            double[,] result = new double[dates.GetLength(0), dates.GetLength(1)];

            for (int row = 0; row < dates.GetLength(0); row += 1)
            {
                for (int col = 0; col < dates.GetLength(1); col += 1)
                {
                    result[row, col] = curve.InterpAtDate(dates[row, col]);
                }
            }
            return result;
        }


        [QuantSAExcelFunction(Description = "Create a curve simulator based on principle components.",
            Name = "QSA.CreatePCACurveSimulator",
            HasGeneratedVersion = true,
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreatePCACurveSimulator.html")]
        public static object CreatePCACurveSimulator([ExcelArgument(Description = "The date from which the curve dates will be calculated.")]Date anchorDate,
            [ExcelArgument(Description = "The starting rates.  Must be the same length as the elements in the component vectors.")]double[] initialRates,
            [ExcelArgument(Description = "An array of times at which each rate applies.  Each value must be valid tenor description.  The length must be the same as each component and 'initialRates'")]Tenor[] tenors,
            [ExcelArgument(Description = "The componenents.  Stack the components in columns side by side or rows one underneath each other.")]double[,] components,
            [ExcelArgument(Description = "The volatility for each component.  Must be the same length as the number of components.")]double[] vols,
            [ExcelArgument(Description = "All rates will be multiplied by this amount.  This should almost always be 1.0.")]double multiplier,
            [ExcelArgument(Description = "Indicates if the PCA was done on relative moves.  If not then it was done on absolute moves.")]bool useRelative,
            [ExcelArgument(Description = "Should simulated rates be floored at zero?  This only applies to absolute moves, the default is 'True'.")]bool floorAtZero)
        {
            return new PCACurveSimulator(anchorDate, initialRates,
                tenors, components, vols, multiplier, useRelative, floorAtZero);
        }


        [QuantSAExcelFunction(Description = "Gets a block of principle component simulated rates.  Each row is a curve at a simulation date.",
            Name = "QSA.PCACurveSimulatorGetRates",
            HasGeneratedVersion = true,
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/PCACurveSimulatorGetRates.html")]
        public static double[,] PCACurveSimulatorGetRates([ExcelArgument(Description = "The simulator.")]PCACurveSimulator simulator,
            [ExcelArgument(Description = "A list of increasing dates.")]Date[] simulationDates,
            [ExcelArgument(Description = "The tenors at which the rates are required.  These do not need to be the same as used to do the PCA.")]Tenor[] requiredTenors)
        {
            return simulator.GetSimulatedRates(simulationDates, requiredTenors);
        }


        [QuantSAExcelFunction(Description = "Create a curve of dates and rates.",
            Name = "QSA.CreateDatesAndRatesCurve",
            HasGeneratedVersion = true,
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateDatesAndRatesCurve.html")]
        public static IDiscountingSource CreateDatesAndRatesCurve([ExcelArgument(Description = "The dates at which the rates apply.")]Date[] dates,
            [ExcelArgument(Description = "The rates.")]double[] rates,
            [QuantSAExcelArgument(Description = "Optional: The currency that this curve can be used for discounting.  Leave blank to use for any currency.", Default = "Currency.ANY")]Currency currency)
        {
            return new DatesAndRates(currency, dates[0], dates, rates);
        }



        [QuantSAExcelFunction(Description = "Get the covariance in log returns from a blob of curves.",
            Name = "QSA.CovarianceFromCurves",
            HasGeneratedVersion = true,
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CovarianceFromCurves.html")]
        public static double[,] CovarianceFromCurves([ExcelArgument(Description = "Blob of curves, each row is a curve of the same length.")]double[,] curves)
        {
            return PCA.CovarianceFromCurves(curves);
        }


        [QuantSAExcelFunction(Description = "Perform a PCA on the log returns of a blob of curves.",
            Name = "QSA.PCAFromCurves",
            HasGeneratedVersion = true,
            Category = "QSA.Curves",
            ExampleSheet = "PCAExample.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/PCAFromCurves.html")]
        public static double[,] PCAFromCurves([ExcelArgument(Description = "Blob of curves, each row is a curve of the same length.")]double[,] curves,
            [ExcelArgument(Description = "Indicates if the PCA is to be done on relative moves.  If not then it will be done on absolute moves.")]bool useRelative)
        {
            return PCA.PCAFromCurves(curves, useRelative);
        }
    }
}
