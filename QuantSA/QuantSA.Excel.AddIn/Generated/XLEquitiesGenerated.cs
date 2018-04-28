using System;
using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.General;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLEquitiesGenerated
    {
        [QuantSAExcelFunction(Name = "QSA.CreateEquityModel", IsGeneratedVersion = true)]
        public static object _CreateEquityModel(string objectName,
            object[,] discountCurve,
            object[,] shares,
            object[,] spotPrices,
            object[,] volatilities,
            object[,] divYields,
            object[,] correlations,
            object[,] rateForecastCurves)
        {
            try
            {
                var _discountCurve = XU.GetObject0D<IDiscountingSource>(discountCurve, "discountCurve");
                var _shares = XU.GetSpecialType1D<Share>(shares, "shares");
                var _spotPrices = XU.GetDouble1D(spotPrices, "spotPrices");
                var _volatilities = XU.GetDouble1D(volatilities, "volatilities");
                var _divYields = XU.GetDouble1D(divYields, "divYields");
                var _correlations = XU.GetDouble2D(correlations, "correlations");
                var _rateForecastCurves = XU.GetObject1D<IFloatingRateSource>(rateForecastCurves, "rateForecastCurves");
                var _result = XLEquities.CreateEquityModel(_discountCurve, _shares, _spotPrices, _volatilities,
                    _divYields, _correlations, _rateForecastCurves);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateEquityCall", IsGeneratedVersion = true)]
        public static object _CreateEquityCall(string objectName,
            object[,] share,
            object[,] exerciseDate,
            object[,] strike)
        {
            try
            {
                var _share = XU.GetSpecialType0D<Share>(share, "share");
                var _exerciseDate = XU.GetDate0D(exerciseDate, "exerciseDate");
                var _strike = XU.GetDouble0D(strike, "strike");
                var _result = XLEquities.CreateEquityCall(_share, _exerciseDate, _strike);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }
    }
}