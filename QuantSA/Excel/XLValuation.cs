using ExcelDna.Integration;
using QuantSA.General;
using QuantSA.Valuation;
using System;
using System.Collections.Generic;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLValuation
    {
        [QuantSAExcelFunction(Description = "Create a curve based valuation model.",
        Name = "QSA.CreateCurveModel",
        Category = "QSA.Valuation",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/CreateCurveModel.html")]
        public static object CreateCurveModel([ExcelArgument(Description = "Name of object")]String name,
        [ExcelArgument(Description = "The discounting curve")]object[,] discountCurve,
        [ExcelArgument(Description = "The floating rate forecast curves for all the rates that the products in the portfolio will need.")]object[,] rateForecastCurves,
        [ExcelArgument(Description = "The FX rate forecast curves for all the cashflow currencies other than the discounting currency.")]object[,] fxForecastCurves)
        {
            try
            {
                DeterminsiticCurves model = new DeterminsiticCurves(XU.GetObjects0D<IDiscountingSource>(discountCurve, "discountCurve"));
                model.AddRateForecast(XU.GetObjects1D<IFloatingRateSource>(rateForecastCurves, "rateForecastCurves"));
                model.AddFXForecast(XU.GetObjects1D<IFXSource>(fxForecastCurves, "fxForecastCurves"));
                return ObjectMap.Instance.AddObject(name, model);
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "Perform a general valuation.",
        Name = "QSA.Value",
        Category = "QSA.Valuation",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/Value.html")]
        public static object Value([ExcelArgument(Description = "Name of object")]String name,
        [ExcelArgument(Description = "A list of products.")]object[,] products,
        [ExcelArgument(Description = "The value date.")]object[,] valueDate,
        [ExcelArgument(Description = "A model able to handle all the market observables required to calculate the cashflows in the portfolio.")]object[,] model,
        [ExcelArgument(Description = "Optional.  The number of simulations required if the model requires simulation.  If left blank will use a default value depending on the model.")]object[,] nSims)
        {
            try
            {
                int N = (nSims[0, 0] is ExcelMissing) ? 1 : XU.GetInts0D(nSims, "nSims");
                
                Coordinator coordinator = new Coordinator(XU.GetObjects0D<NumeraireSimulator>(model, "model"), 
                     new List<Simulator>(), N);
                double value = coordinator.Value(XU.GetObjects1D<Product>(products, "products"), 
                    XU.GetDates0D(valueDate, "valueDate"));
                ResultStore result = new ResultStore();
                result.Add("value", value);
                return XU.AddObject(name, result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }
    }
}

