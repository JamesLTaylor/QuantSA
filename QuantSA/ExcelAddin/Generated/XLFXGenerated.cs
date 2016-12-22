using System;
using XU = QuantSA.Excel.ExcelUtilities;
using QuantSA.General;
using QuantSA.Valuation;
using QuantSA.ExcelFunctions;
using QuantSA.Excel.Common;

namespace QuantSA.Excel
{
    public class XLFXGenerated
    {

        [QuantSAExcelFunction(Name = "QSA.CreateFXForecastCurve", IsGeneratedVersion = true)]
        public static object _CreateFXForecastCurve(string objectName,
                            object[,] baseCurrency,
                            object[,] counterCurrency,
                            object[,] fxRateAtAnchorDate,
                            object[,] baseCurrencyFXBasisCurve,
                            object[,] counterCurrencyFXBasisCurve)
        {
            try
            {
                Currency _baseCurrency = XU.GetCurrency0D(baseCurrency, "baseCurrency");
                Currency _counterCurrency = XU.GetCurrency0D(counterCurrency, "counterCurrency");
                Double _fxRateAtAnchorDate = XU.GetDouble0D(fxRateAtAnchorDate, "fxRateAtAnchorDate");
                IDiscountingSource _baseCurrencyFXBasisCurve = XU.GetObject0D<IDiscountingSource>(baseCurrencyFXBasisCurve, "baseCurrencyFXBasisCurve");
                IDiscountingSource _counterCurrencyFXBasisCurve = XU.GetObject0D<IDiscountingSource>(counterCurrencyFXBasisCurve, "counterCurrencyFXBasisCurve");
                Object _result = XLFX.CreateFXForecastCurve(_baseCurrency, _counterCurrency, _fxRateAtAnchorDate, _baseCurrencyFXBasisCurve, _counterCurrencyFXBasisCurve);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.GetFXRate", IsGeneratedVersion = true)]
        public static object _GetFXRate(object[,] fxCurve,
                            object[,] date)
        {
            try
            {
                IFXSource _fxCurve = XU.GetObject0D<IFXSource>(fxCurve, "fxCurve");
                Date _date = XU.GetDate0D(date, "date");
                Double _result = XLFX.GetFXRate(_fxCurve, _date);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

    }
}
