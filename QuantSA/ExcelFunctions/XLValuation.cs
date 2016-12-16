using ExcelDna.Integration;
using QuantSA.General;
using QuantSA.Valuation;
using System.Collections.Generic;
using QuantSA.Excel;

namespace QuantSA.ExcelFunctions
{
    public class XLValuation
    {
        [QuantSAExcelFunction(Description = "Create a curve based valuation model.",
            Name = "QSA.CreateCurveModel",
            HasGeneratedVersion = true,
            Category = "QSA.Valuation",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateCurveModel.html")]
        public static NumeraireSimulator CreateCurveModel([ExcelArgument(Description = "The discounting curve")]IDiscountingSource discountCurve,
            [ExcelArgument(Description = "The floating rate forecast curves for all the rates that the products in the portfolio will need.")]IFloatingRateSource[] rateForecastCurves,
            [ExcelArgument(Description = "The FX rate forecast curves for all the cashflow currencies other than the discounting currency.")]IFXSource[] fxForecastCurves)
        {
            DeterminsiticCurves model = new DeterminsiticCurves(discountCurve);
                model.AddRateForecast(rateForecastCurves);
                model.AddFXForecast(fxForecastCurves);
            return model;
        }

        [QuantSAExcelFunction(Description = "Perform a general valuation.",
            Name = "QSA.Value",
            HasGeneratedVersion = true,
            Category = "QSA.Valuation",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/Value.html")]
        public static ResultStore Value([ExcelArgument(Description = "A list of products.")]Product[] products,
            [QuantSAExcelArgument(Description = "The value date.")]Date valueDate,
            [QuantSAExcelArgument(Description = "A model able to handle all the market observables required to calculate the cashflows in the portfolio.")]NumeraireSimulator model,
            [QuantSAExcelArgument(Description = "Optional.  The number of simulations required if the model requires simulation.  If left blank will use a default value depending on the model.", Default = "1")]int nSims)
        {
            //int N = (nSims[0, 0] is ExcelMissing) ? 1 : XU.GetInt0D(nSims, "nSims");
                
            Coordinator coordinator = new Coordinator(model, new List<Simulator>(), nSims);
            double value = coordinator.Value(products, valueDate);
            ResultStore result = new ResultStore();
            result.Add("value", value);
            return result;
        }


        [QuantSAExcelFunction(Description = "Calculate the expected posiitive exposure for a general portfolio",
            Name = "QSA.EPE",
            HasGeneratedVersion = true,
            Category = "QSA.Valuation",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/EPE.html")]
        public static double[] EPE([ExcelArgument(Description = "A list of products.")]Product[] products,
            [ExcelArgument(Description = "The value date.")]Date valueDate,
            [ExcelArgument(Description = "The dates at which the expected positive exposure is required.")]Date[] forwardValueDates,
            [ExcelArgument(Description = "A model able to handle all the market observables required to calculate the cashflows in the portfolio.")]NumeraireSimulator model,
            [ExcelArgument(Description = "The number of simulations required.")]int nSims)
        {
            Coordinator coordinator = new Coordinator(model, new List<Simulator>(), nSims);
            return coordinator.EPE(products, valueDate, forwardValueDates);
        }
    }
}

