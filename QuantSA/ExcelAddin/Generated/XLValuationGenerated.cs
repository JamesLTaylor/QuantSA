using System;
using XU = QuantSA.Excel.ExcelUtilities;
using QuantSA.General;
using QuantSA.Valuation;
using QuantSA.ExcelFunctions;
using QuantSA.Excel.Common;

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
                IDiscountingSource _discountCurve = XU.GetObject0D<IDiscountingSource>(discountCurve, "discountCurve");
                IFloatingRateSource[] _rateForecastCurves = XU.GetObject1D<IFloatingRateSource>(rateForecastCurves, "rateForecastCurves");
                IFXSource[] _fxForecastCurves = XU.GetObject1D<IFXSource>(fxForecastCurves, "fxForecastCurves");
                NumeraireSimulator _result = XLValuation.CreateCurveModel(_discountCurve, _rateForecastCurves, _fxForecastCurves);
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
                Product[] _products = XU.GetObject1D<Product>(products, "products");
                Date _valueDate = XU.GetDate0D(valueDate, "valueDate");
                NumeraireSimulator _model = XU.GetObject0D<NumeraireSimulator>(model, "model");
                Int32 _nSims = XU.GetInt320D(nSims, "nSims", 1);
                ResultStore _result = XLValuation.Value(_products, _valueDate, _model, _nSims);
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
                Product[] _products = XU.GetObject1D<Product>(products, "products");
                Date _valueDate = XU.GetDate0D(valueDate, "valueDate");
                Date[] _forwardValueDates = XU.GetDate1D(forwardValueDates, "forwardValueDates");
                NumeraireSimulator _model = XU.GetObject0D<NumeraireSimulator>(model, "model");
                Int32 _nSims = XU.GetInt320D(nSims, "nSims");
                Double[] _result = XLValuation.EPE(_products, _valueDate, _forwardValueDates, _model, _nSims);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error2D(e);
            }
        }

    }
}
