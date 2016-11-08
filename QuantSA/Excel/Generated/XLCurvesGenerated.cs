using System;
using XU = QuantSA.Excel.ExcelUtilities;
using QuantSA.General;
using QuantSA.Valuation;

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
                Date _anchorDate = XU.GetDate0D(anchorDate, "anchorDate");
                Date[] _dates = XU.GetDate1D(dates, "dates");
                Double[] _rates = XU.GetDouble1D(rates, "rates");
                ICurve _result = XLCurves.FitCurveNelsonSiegel(_anchorDate,_dates,_rates);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CurveInterp", IsGeneratedVersion = true)]
        public static object[,] _CurveInterp(                            object[,] curve,
                            object[,] dates)
        {
            try
            {
                ICurve _curve = XU.GetObject0D<ICurve>(curve, "curve");
                Date[,] _dates = XU.GetDate2D(dates, "dates");
                Double[,] _result = XLCurves.CurveInterp(_curve,_dates);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }

    }
}
