using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.General;
using QuantSA.General.Dates;
using QuantSA.Valuation;
using System;
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
                Currency _baseCurrency = XU.GetSpecialType0D<Currency>(baseCurrency, "baseCurrency");
                Currency _counterCurrency = XU.GetSpecialType0D<Currency>(counterCurrency, "counterCurrency");
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
                Date _anchorDate = XU.GetDate0D(anchorDate, "anchorDate");
                Currency _numeraireCcy = XU.GetSpecialType0D<Currency>(numeraireCcy, "numeraireCcy");
                HullWhite1F[] _rateSimulators = XU.GetObject1D<HullWhite1F>(rateSimulators, "rateSimulators");
                Currency[] _currencies = XU.GetSpecialType1D<Currency>(currencies, "currencies");
                Double[] _spots = XU.GetDouble1D(spots, "spots");
                Double[] _vols = XU.GetDouble1D(vols, "vols");
                Double[,] _correlations = XU.GetDouble2D(correlations, "correlations");
                NumeraireSimulator _result = XLFX.CreateMultiHWAndFXToy(_anchorDate, _numeraireCcy, _rateSimulators, _currencies, _spots, _vols, _correlations);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

    }
}
