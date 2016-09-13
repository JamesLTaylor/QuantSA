using ExcelDna.Integration;
using QuantSA;
using System;

namespace QuantSA.Excel
{
    public static class XLCurves
    {
        [QuantSAExcelFunction(Description = "Create a best fit Nelson Siegel curve.  Can be used anywhere as a curve.",
        Name = "QSA.FitCurveNelsonSiegel",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static string FitCurveNelsonSiegel([ExcelArgument(Description = "Name of object")]String name,
                [ExcelArgument(Description = "The date at which the resultant curve will be anchored.  Can set to zero.")]double anchorDate,
                [ExcelArgument(Description = "dates at which rates apply.")]double[] dates,
                [ExcelArgument(Description = "Rates to be fitted")]Double[] rates)
        {
            try {
                NelsonSiegel curve = NelsonSiegel.Fit(ExcelUtilites.GetDates(anchorDate), ExcelUtilites.GetDates(dates), rates);
                return ObjectMap.Instance.AddObject(name, curve);
            } catch (Exception e)
            {
                return e.Message;
            }
        }

        [QuantSAExcelFunction(Description = "Find the interpolated value of any QuantSA created curve",
        Name = "QSA.CurveInterp",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static object[,] CurveInterp([ExcelArgument(Description = "The name of the curve to interpolate")]String name,
        [ExcelArgument(Description = "The dates at which interpolated rates are required.")]double[,] dates)
        {
            try {
                ICurve curve = (ICurve)ObjectMap.Instance.GetObjectFromID(name);
                Date[,] dtDates = ExcelUtilites.GetDates(dates);
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
                return ExcelUtilites.Error2D(e);
            }
        }


        [ExcelFunction(Description = "",
        Name = "QSA.CreatePCACurveSimulator",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static object CreatePCACurveSimulator([ExcelArgument(Description = "")]string simulatorName,
            [ExcelArgument(Description = "")]double anchorDate,
            [ExcelArgument(Description = "")]double[] initialRates,
            [ExcelArgument(Description = "")]double[] tenorMonths,
            [ExcelArgument(Description = "")]double[,] components,
            [ExcelArgument(Description = "")]double[] vols,
            [ExcelArgument(Description = "")]double multiplier)

        {
            try
            {
                int[] tenorMonthsInt = new int[tenorMonths.Length];
                for (int i = 0; i < tenorMonths.Length; i++) { tenorMonthsInt[i] = (int)tenorMonths[i]; }
                PCACurveSimulator curveSimulator = new PCACurveSimulator(ExcelUtilites.GetDates(anchorDate), 
                    initialRates, tenorMonthsInt, components, vols, multiplier);
                return ObjectMap.Instance.AddObject(simulatorName, curveSimulator);                
            }
            catch (Exception e)
            {
                return ExcelUtilites.Error2D(e);                
            }
        }


        [ExcelFunction(Description = "",
        Name = "QSA.PCACurveSimulatorGetRates",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "https://www.google.co.za")]
        public static object[,] PCACurveSimulatorGetRates([ExcelArgument(Description = "")]string simulatorName,
            [ExcelArgument(Description = "")]double[] simulationDates,
            [ExcelArgument(Description = "")]double[] requiredTenorMonths)

        {
            try
            {
                int[] tenorMonthsInt = new int[requiredTenorMonths.Length];
                for (int i = 0; i < requiredTenorMonths.Length; i++) { tenorMonthsInt[i] = (int)requiredTenorMonths[i]; }


                PCACurveSimulator curveSimulator = (PCACurveSimulator)ObjectMap.Instance.GetObjectFromID(simulatorName);
                double[,] result = curveSimulator.GetSimulatedRates(ExcelUtilites.GetDates(simulationDates), tenorMonthsInt);
                return ExcelUtilites.GetObjects(result);
            }

            catch (Exception e)
            {
                return ExcelUtilites.Error2D(e);
            }
        }


    }
}
