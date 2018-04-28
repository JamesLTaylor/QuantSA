using System;
using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.General;
using QuantSA.Valuation;
using XU = QuantSA.Excel.ExcelUtilities;

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
                var _baseCurrency = XU.GetSpecialType0D<Currency>(baseCurrency, "baseCurrency");
                var _counterCurrency = XU.GetSpecialType0D<Currency>(counterCurrency, "counterCurrency");
                var _fxRateAtAnchorDate = XU.GetDouble0D(fxRateAtAnchorDate, "fxRateAtAnchorDate");
                var _baseCurrencyFXBasisCurve =
                    XU.GetObject0D<IDiscountingSource>(baseCurrencyFXBasisCurve, "baseCurrencyFXBasisCurve");
                var _counterCurrencyFXBasisCurve =
                    XU.GetObject0D<IDiscountingSource>(counterCurrencyFXBasisCurve, "counterCurrencyFXBasisCurve");
                var _result = XLFX.CreateFXForecastCurve(_baseCurrency, _counterCurrency, _fxRateAtAnchorDate,
                    _baseCurrencyFXBasisCurve, _counterCurrencyFXBasisCurve);
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
                var _fxCurve = XU.GetObject0D<IFXSource>(fxCurve, "fxCurve");
                var _date = XU.GetDate0D(date, "date");
                var _result = XLFX.GetFXRate(_fxCurve, _date);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateMultiHWAndFXToy", IsGeneratedVersion = true)]
        public static object _CreateMultiHWAndFXToy(string objectName,
            object[,] anchorDate,
            object[,] numeraireCcy,
            object[,] rateSimulators,
            object[,] currencies,
            object[,] spots,
            object[,] vols,
            object[,] correlations)
        {
            try
            {
                var _anchorDate = XU.GetDate0D(anchorDate, "anchorDate");
                var _numeraireCcy = XU.GetSpecialType0D<Currency>(numeraireCcy, "numeraireCcy");
                var _rateSimulators = XU.GetObject1D<HullWhite1F>(rateSimulators, "rateSimulators");
                var _currencies = XU.GetSpecialType1D<Currency>(currencies, "currencies");
                var _spots = XU.GetDouble1D(spots, "spots");
                var _vols = XU.GetDouble1D(vols, "vols");
                var _correlations = XU.GetDouble2D(correlations, "correlations");
                var _result = XLFX.CreateMultiHWAndFXToy(_anchorDate, _numeraireCcy, _rateSimulators, _currencies,
                    _spots, _vols, _correlations);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }
    }
}