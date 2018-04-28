using System;
using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.General;
using QuantSA.Valuation;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLValuationGenerated
    {
        [QuantSAExcelFunction(Name = "QSA.CreateCurveModel", IsGeneratedVersion = true)]
        public static object _CreateCurveModel(string objectName,
            object[,] discountCurve,
            object[,] rateForecastCurves,
            object[,] fxForecastCurves)
        {
            try
            {
                var _discountCurve = XU.GetObject0D<IDiscountingSource>(discountCurve, "discountCurve");
                var _rateForecastCurves = XU.GetObject1D<IFloatingRateSource>(rateForecastCurves, "rateForecastCurves");
                var _fxForecastCurves = XU.GetObject1D<IFXSource>(fxForecastCurves, "fxForecastCurves");
                var _result = XLValuation.CreateCurveModel(_discountCurve, _rateForecastCurves, _fxForecastCurves);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.Value", IsGeneratedVersion = true)]
        public static object _Value(string objectName,
            object[,] products,
            object[,] valueDate,
            object[,] model,
            object[,] nSims)
        {
            try
            {
                var _products = XU.GetObject1D<Product>(products, "products");
                var _valueDate = XU.GetDate0D(valueDate, "valueDate");
                var _model = XU.GetObject0D<NumeraireSimulator>(model, "model");
                var _nSims = XU.GetInt320D(nSims, "nSims", 1);
                var _result = XLValuation.Value(_products, _valueDate, _model, _nSims);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.EPE", IsGeneratedVersion = true)]
        public static object[,] _EPE(object[,] products,
            object[,] valueDate,
            object[,] forwardValueDates,
            object[,] model,
            object[,] nSims)
        {
            try
            {
                var _products = XU.GetObject1D<Product>(products, "products");
                var _valueDate = XU.GetDate0D(valueDate, "valueDate");
                var _forwardValueDates = XU.GetDate1D(forwardValueDates, "forwardValueDates");
                var _model = XU.GetObject0D<NumeraireSimulator>(model, "model");
                var _nSims = XU.GetInt320D(nSims, "nSims");
                var _result = XLValuation.EPE(_products, _valueDate, _forwardValueDates, _model, _nSims);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.PFE", IsGeneratedVersion = true)]
        public static object[,] _PFE(object[,] products,
            object[,] valueDate,
            object[,] forwardValueDates,
            object[,] requiredPecentiles,
            object[,] model,
            object[,] nSims)
        {
            try
            {
                var _products = XU.GetObject1D<Product>(products, "products");
                var _valueDate = XU.GetDate0D(valueDate, "valueDate");
                var _forwardValueDates = XU.GetDate1D(forwardValueDates, "forwardValueDates");
                var _requiredPecentiles = XU.GetDouble1D(requiredPecentiles, "requiredPecentiles");
                var _model = XU.GetObject0D<NumeraireSimulator>(model, "model");
                var _nSims = XU.GetInt320D(nSims, "nSims");
                var _result = XLValuation.PFE(_products, _valueDate, _forwardValueDates, _requiredPecentiles, _model,
                    _nSims);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }
    }
}