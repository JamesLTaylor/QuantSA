using System;
using ExcelDna.Integration;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.CurveTools;
using QuantSA.Core.DataAnalysis;
using QuantSA.Core.MarketData;
using QuantSA.Excel.Shared;
using QuantSA.Shared.CurvesAndSurfaces;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.ExcelFunctions
{
    public static class XLCurves
    {
        [QuantSAExcelFunction(
            Description = "Create a Nelson Siegel curve from parameters.  Can be used anywhere as a curve.",
            Name = "QSA.CreateNelsonSiegel",
            HasGeneratedVersion = true,
            ExampleSheet = "FitCurveNelsonSiegel.xlsx",
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/FitCurveNelsonSiegel.html")]
        public static ICurve CreateNelsonSiegel(
            [ExcelArgument(Description = "The date at which the resultant curve will be anchored.")]
            Date anchorDate,
            [ExcelArgument(Description = "")] double beta0,
            [ExcelArgument(Description = "")] double beta1,
            [ExcelArgument(Description = "")] double beta2,
            [ExcelArgument(Description = "")] double tau)
        {
            return new NelsonSiegel(anchorDate, beta0, beta1, beta2, tau);
        }

        [QuantSAExcelFunction(
            Description = "Create a best fit Nelson Siegel curve.  Can be used anywhere as a curve. (Curve)",
            Name = "QSA.FitCurveNelsonSiegel",
            HasGeneratedVersion = true,
            ExampleSheet = "FitCurveNelsonSiegel.xlsx",
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/FitCurveNelsonSiegel.html")]
        public static ICurve FitCurveNelsonSiegel(
            [ExcelArgument(Description =
                "The date at which the resultant curve will be anchored.  Can be set to zero.")]
            Date anchorDate,
            [ExcelArgument(Description = "The dates at which rates apply.")]
            Date[] dates,
            [ExcelArgument(Description = "The rates to be fitted")]
            double[] rates)
        {
            return NelsonSiegel.Fit(anchorDate, dates, rates);
        }


        [QuantSAExcelFunction(Description = "Find the interpolated value of any QuantSA created curve.",
            Name = "QSA.CurveInterp",
            HasGeneratedVersion = true,
            ExampleSheet = "FitCurveNelsonSiegel.xlsx",
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CurveInterp.html")]
        public static double[,] CurveInterp([ExcelArgument(Description = "The curve to interpolate.")]
            ICurve curve,
            [ExcelArgument(Description = "The dates at which interpolated rates are required.")]
            Date[,] dates)
        {
            var result = new double[dates.GetLength(0), dates.GetLength(1)];

            for (var row = 0; row < dates.GetLength(0); row += 1)
            for (var col = 0; col < dates.GetLength(1); col += 1)
                result[row, col] = curve.InterpAtDate(dates[row, col]);
            return result;
        }


        [QuantSAExcelFunction(Description = "Create a curve simulator based on principle components.",
            Name = "QSA.CreatePCACurveSimulator",
            HasGeneratedVersion = true,
            ExampleSheet = "PCA.xlsx",
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreatePCACurveSimulator.html")]
        public static PCACurveSimulator CreatePCACurveSimulator(
            [ExcelArgument(Description = "The date from which the curve dates will be calculated.")]
            Date anchorDate,
            [ExcelArgument(Description =
                "The starting rates.  Must be the same length as the elements in the component vectors.")]
            double[] initialRates,
            [ExcelArgument(Description =
                "An array of times at which each rate applies.  Each value must be valid tenor description.  The length must be the same as each component and 'initialRates'")]
            Tenor[] tenors,
            [ExcelArgument(Description =
                "The components.  Stack the components in columns side by side or rows one underneath each other.")]
            double[,] components,
            [ExcelArgument(Description =
                "The volatility for each component.  Must be the same length as the number of components.")]
            double[] vols,
            [ExcelArgument(Description =
                "All rates will be multiplied by this amount.  This should almost always be 1.0.")]
            double multiplier,
            [ExcelArgument(Description =
                "Indicates if the PCA was done on relative moves.  If not then it was done on absolute moves.")]
            bool useRelative,
            [ExcelArgument(Description =
                "Should simulated rates be floored at zero?  This only applies to absolute moves, the default is 'True'.")]
            bool floorAtZero)
        {
            return new PCACurveSimulator(anchorDate, initialRates,
                tenors, components, vols, multiplier, useRelative, floorAtZero);
        }


        [QuantSAExcelFunction(
            Description =
                "Gets a block of principle component simulated rates.  Each row is a curve at a simulation date.",
            Name = "QSA.PCACurveSimulatorGetRates",
            HasGeneratedVersion = true,
            ExampleSheet = "PCA.xlsx",
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/PCACurveSimulatorGetRates.html")]
        public static double[,] PCACurveSimulatorGetRates([ExcelArgument(Description = "The simulator.")]
            PCACurveSimulator simulator,
            [ExcelArgument(Description = "A list of increasing dates.")]
            Date[] simulationDates,
            [ExcelArgument(Description =
                "The tenors at which the rates are required.  These do not need to be the same as used to do the PCA.")]
            Tenor[] requiredTenors)
        {
            return simulator.GetSimulatedRates(simulationDates, requiredTenors);
        }


        [QuantSAExcelFunction(Description = "Create a curve of dates and rates.",
            Name = "QSA.CreateDatesAndRatesCurve",
            HasGeneratedVersion = true,
            ExampleSheet = "GeneralSwap.xlsx",
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateDatesAndRatesCurve.html")]
        public static IDiscountingSource CreateDatesAndRatesCurve(
            [ExcelArgument(Description = "The dates at which the rates apply.")]
            Date[] dates,
            [ExcelArgument(Description = "The rates.")]
            double[] rates,
            [QuantSAExcelArgument(
                Description =
                    "Optional: The currency that this curve can be used for discounting.  Leave blank to use for any currency.",
                Default = "ZAR")]
            Currency currency)
        {
            for (var i = 1; i < dates.Length; i++)
                if (dates[i].value <= dates[i - 1].value)
                    throw new ArgumentException("Dates must be strictly increasing");
            return new DatesAndRates(currency, dates[0], dates, rates);
        }

        [QuantSAExcelFunction(Description = "Create a forecast curve for a Libor type index.",
            Name = "QSA.CreateRateForecastCurve",
            HasGeneratedVersion = true,
            ExampleSheet = "Caplet.xlsx",
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateRateForecastCurve.html")]
        public static IFloatingRateSource CreateRateForecastCurve(
            [QuantSAExcelArgument(Description = "The anchor date of the curve.")]
            Date anchorDate,
            [ExcelArgument(Description = "The dates at which the rates apply.")]
            Date[] dates,
            [ExcelArgument(Description = "The rates.")]
            double[] rates,
            [QuantSAExcelArgument(Description = "The index that this curve forecasts.", Default = "ZAR.JIBAR.3M")]
            FloatRateIndex floatRateIndex)
        {
            for (var i = 1; i < dates.Length; i++)
                if (dates[i].value <= dates[i - 1].value)
                    throw new ArgumentException($"{nameof(dates)} must be strictly increasing");
            if (dates.Length != rates.Length)
                throw new ArgumentException($"{nameof(dates)} and {nameof(rates)} must be the same length");
            return new ForecastCurve(anchorDate, floatRateIndex, dates, rates);
        }


        [QuantSAExcelFunction(Description = "Get the covariance in log returns from a blob of curves.",
            Name = "QSA.CovarianceFromCurves",
            HasGeneratedVersion = true,
            ExampleSheet = "PCA.xlsx",
            Category = "QSA.Curves",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CovarianceFromCurves.html")]
        public static double[,] CovarianceFromCurves(
            [ExcelArgument(Description = "Blob of curves, each row is a curve of the same length.")]
            double[,] curves)
        {
            return PCA.CovarianceFromCurves(curves);
        }


        [QuantSAExcelFunction(Description = "Perform a PCA on the log returns of a blob of curves.",
            Name = "QSA.PCAFromCurves",
            HasGeneratedVersion = true,
            Category = "QSA.Curves",
            ExampleSheet = "PCA.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/PCAFromCurves.html")]
        public static double[,] PCAFromCurves(
            [ExcelArgument(Description = "Blob of curves, each row is a curve of the same length.")]
            double[,] curves,
            [ExcelArgument(Description =
                "Indicates if the PCA is to be done on relative moves.  If not then it will be done on absolute moves.")]
            bool useRelative)
        {
            return PCA.PCAFromCurves(curves, useRelative);
        }
    }
}