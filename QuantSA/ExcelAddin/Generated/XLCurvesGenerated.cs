using System;
using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.Primitives;
using QuantSA.Primitives.CurveTools;
using QuantSA.Primitives.Dates;
using XU = QuantSA.Excel.Addin.ExcelUtilities;

namespace QuantSA.Excel.Addin.Generated
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
                Date _anchorDate = XU.GetDate0D(anchorDate, "anchorDate");
                Date[] _dates = XU.GetDate1D(dates, "dates");
                Double[] _rates = XU.GetDouble1D(rates, "rates");
                ICurve _result = XLCurves.FitCurveNelsonSiegel(_anchorDate, _dates, _rates);
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
                ICurve _curve = XU.GetObject0D<ICurve>(curve, "curve");
                Date[,] _dates = XU.GetDate2D(dates, "dates");
                Double[,] _result = XLCurves.CurveInterp(_curve, _dates);
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
                Date _anchorDate = XU.GetDate0D(anchorDate, "anchorDate");
                Double[] _initialRates = XU.GetDouble1D(initialRates, "initialRates");
                Tenor[] _tenors = XU.GetSpecialType1D<Tenor>(tenors, "tenors");
                Double[,] _components = XU.GetDouble2D(components, "components");
                Double[] _vols = XU.GetDouble1D(vols, "vols");
                Double _multiplier = XU.GetDouble0D(multiplier, "multiplier");
                Boolean _useRelative = XU.GetBoolean0D(useRelative, "useRelative");
                Boolean _floorAtZero = XU.GetBoolean0D(floorAtZero, "floorAtZero");
                Object _result = XLCurves.CreatePCACurveSimulator(_anchorDate, _initialRates, _tenors, _components, _vols, _multiplier, _useRelative, _floorAtZero);
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
                PCACurveSimulator _simulator = XU.GetObject0D<PCACurveSimulator>(simulator, "simulator");
                Date[] _simulationDates = XU.GetDate1D(simulationDates, "simulationDates");
                Tenor[] _requiredTenors = XU.GetSpecialType1D<Tenor>(requiredTenors, "requiredTenors");
                Double[,] _result = XLCurves.PCACurveSimulatorGetRates(_simulator, _simulationDates, _requiredTenors);
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
                Date[] _dates = XU.GetDate1D(dates, "dates");
                Double[] _rates = XU.GetDouble1D(rates, "rates");
                Currency _currency = XU.GetSpecialType0D<Currency>(currency, "currency", Currency.ANY);
                IDiscountingSource _result = XLCurves.CreateDatesAndRatesCurve(_dates, _rates, _currency);
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
                Double[,] _curves = XU.GetDouble2D(curves, "curves");
                Double[,] _result = XLCurves.CovarianceFromCurves(_curves);
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
                Double[,] _curves = XU.GetDouble2D(curves, "curves");
                Boolean _useRelative = XU.GetBoolean0D(useRelative, "useRelative");
                Double[,] _result = XLCurves.PCAFromCurves(_curves, _useRelative);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }

    }
}
