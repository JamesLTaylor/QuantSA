using System;
using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.General;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLCurvesGenerated
    {
        [QuantSAExcelFunction(Name = "QSA.FitCurveNelsonSiegel", IsGeneratedVersion = true)]
        public static object _FitCurveNelsonSiegel(string objectName,
            object[,] anchorDate,
            object[,] dates,
            object[,] rates)
        {
            try
            {
                var _anchorDate = XU.GetDate0D(anchorDate, "anchorDate");
                var _dates = XU.GetDate1D(dates, "dates");
                var _rates = XU.GetDouble1D(rates, "rates");
                var _result = XLCurves.FitCurveNelsonSiegel(_anchorDate, _dates, _rates);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CurveInterp", IsGeneratedVersion = true)]
        public static object[,] _CurveInterp(object[,] curve,
            object[,] dates)
        {
            try
            {
                var _curve = XU.GetObject0D<ICurve>(curve, "curve");
                var _dates = XU.GetDate2D(dates, "dates");
                var _result = XLCurves.CurveInterp(_curve, _dates);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreatePCACurveSimulator", IsGeneratedVersion = true)]
        public static object _CreatePCACurveSimulator(string objectName,
            object[,] anchorDate,
            object[,] initialRates,
            object[,] tenors,
            object[,] components,
            object[,] vols,
            object[,] multiplier,
            object[,] useRelative,
            object[,] floorAtZero)
        {
            try
            {
                var _anchorDate = XU.GetDate0D(anchorDate, "anchorDate");
                var _initialRates = XU.GetDouble1D(initialRates, "initialRates");
                var _tenors = XU.GetSpecialType1D<Tenor>(tenors, "tenors");
                var _components = XU.GetDouble2D(components, "components");
                var _vols = XU.GetDouble1D(vols, "vols");
                var _multiplier = XU.GetDouble0D(multiplier, "multiplier");
                var _useRelative = XU.GetBoolean0D(useRelative, "useRelative");
                var _floorAtZero = XU.GetBoolean0D(floorAtZero, "floorAtZero");
                var _result = XLCurves.CreatePCACurveSimulator(_anchorDate, _initialRates, _tenors, _components, _vols,
                    _multiplier, _useRelative, _floorAtZero);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.PCACurveSimulatorGetRates", IsGeneratedVersion = true)]
        public static object[,] _PCACurveSimulatorGetRates(object[,] simulator,
            object[,] simulationDates,
            object[,] requiredTenors)
        {
            try
            {
                var _simulator = XU.GetObject0D<PCACurveSimulator>(simulator, "simulator");
                var _simulationDates = XU.GetDate1D(simulationDates, "simulationDates");
                var _requiredTenors = XU.GetSpecialType1D<Tenor>(requiredTenors, "requiredTenors");
                var _result = XLCurves.PCACurveSimulatorGetRates(_simulator, _simulationDates, _requiredTenors);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateDatesAndRatesCurve", IsGeneratedVersion = true)]
        public static object _CreateDatesAndRatesCurve(string objectName,
            object[,] dates,
            object[,] rates,
            object[,] currency)
        {
            try
            {
                var _dates = XU.GetDate1D(dates, "dates");
                var _rates = XU.GetDouble1D(rates, "rates");
                var _currency = XU.GetSpecialType0D(currency, "currency", Currency.ANY);
                var _result = XLCurves.CreateDatesAndRatesCurve(_dates, _rates, _currency);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CovarianceFromCurves", IsGeneratedVersion = true)]
        public static object[,] _CovarianceFromCurves(object[,] curves)
        {
            try
            {
                var _curves = XU.GetDouble2D(curves, "curves");
                var _result = XLCurves.CovarianceFromCurves(_curves);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.PCAFromCurves", IsGeneratedVersion = true)]
        public static object[,] _PCAFromCurves(object[,] curves,
            object[,] useRelative)
        {
            try
            {
                var _curves = XU.GetDouble2D(curves, "curves");
                var _useRelative = XU.GetBoolean0D(useRelative, "useRelative");
                var _result = XLCurves.PCAFromCurves(_curves, _useRelative);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }
    }
}