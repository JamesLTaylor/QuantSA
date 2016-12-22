using System;
using XU = QuantSA.Excel.ExcelUtilities;
using QuantSA.General;
using QuantSA.Valuation;
using QuantSA.ExcelFunctions;
using QuantSA.Excel.Common;

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
                IDiscountingSource _discountCurve = XU.GetObject0D<IDiscountingSource>(discountCurve, "discountCurve");
                Share[] _shares = XU.GetShare1D(shares, "shares");
                Double[] _spotPrices = XU.GetDouble1D(spotPrices, "spotPrices");
                Double[] _volatilities = XU.GetDouble1D(volatilities, "volatilities");
                Double[] _divYields = XU.GetDouble1D(divYields, "divYields");
                Double[,] _correlations = XU.GetDouble2D(correlations, "correlations");
                IFloatingRateSource[] _rateForecastCurves = XU.GetObject1D<IFloatingRateSource>(rateForecastCurves, "rateForecastCurves");
                NumeraireSimulator _result = XLEquities.CreateEquityModel(_discountCurve, _shares, _spotPrices, _volatilities, _divYields, _correlations, _rateForecastCurves);
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
                Share _share = XU.GetShare0D(share, "share");
                Date _exerciseDate = XU.GetDate0D(exerciseDate, "exerciseDate");
                Double _strike = XU.GetDouble0D(strike, "strike");
                Product _result = XLEquities.CreateEquityCall(_share, _exerciseDate, _strike);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

    }
}
