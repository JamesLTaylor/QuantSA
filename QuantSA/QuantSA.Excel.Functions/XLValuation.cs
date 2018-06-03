using System.Collections.Generic;
using ExcelDna.Integration;
using QuantSA.Excel.Shared;
using QuantSA.General;
using QuantSA.Primitives.Dates;
using QuantSA.Valuation;

namespace QuantSA.ExcelFunctions
{
    public class XLValuation
    {
        [QuantSAExcelFunction(Description = "Create a curve based valuation model.",
            Name = "QSA.CreateCurveModel",
            HasGeneratedVersion = true,
            Category = "QSA.Valuation",
            ExampleSheet = "GeneralSwap.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateCurveModel.html")]
        public static NumeraireSimulator CreateCurveModel([QuantSAExcelArgument(Description = "The discounting curve")]
            IDiscountingSource discountCurve,
            [QuantSAExcelArgument(Description =
                "The floating rate forecast curves for all the rates that the products in the portfolio will need.")]
            IFloatingRateSource[] rateForecastCurves,
            [QuantSAExcelArgument(Description =
                "The FX rate forecast curves for all the cashflow currencies other than the discounting currency.", Default = null)]
            IFXSource[] fxForecastCurves)
        {
            var model = new DeterminsiticCurves(discountCurve);
            model.AddRateForecast(rateForecastCurves);
            model.AddFXForecast(fxForecastCurves);
            return model;
        }

        [QuantSAExcelFunction(Description = "Perform a general valuation.",
            Name = "QSA.Value",
            HasGeneratedVersion = true,
            Category = "QSA.Valuation",
            ExampleSheet = "GeneralSwap.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/Value.html")]
        public static ResultStore Value([ExcelArgument(Description = "A list of products.")]
            Product[] products,
            [QuantSAExcelArgument(Description = "The value date.")]
            Date valueDate,
            [QuantSAExcelArgument(Description =
                "A model able to handle all the market observables required to calculate the cashflows in the portfolio.")]
            NumeraireSimulator model,
            [QuantSAExcelArgument(
                Description =
                    "Optional.  The number of simulations required if the model requires simulation.  If left blank will use a default value depending on the model.",
                Default = "1")]
            int nSims)
        {
            var coordinator = new Coordinator(model, new List<Simulator>(), nSims);
            var value = coordinator.Value(products, valueDate);
            var result = new ResultStore();
            result.Add("value", value);
            return result;
        }


        [QuantSAExcelFunction(Description = "Calculate the expected positive exposure for a general portfolio",
            Name = "QSA.EPE",
            HasGeneratedVersion = true,
            Category = "QSA.Valuation",
            ExampleSheet = "EPE.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/EPE.html")]
        public static double[] EPE([ExcelArgument(Description = "A list of products.")]
            Product[] products,
            [ExcelArgument(Description = "The value date.")]
            Date valueDate,
            [ExcelArgument(Description = "The dates at which the expected positive exposure is required.")]
            Date[] forwardValueDates,
            [ExcelArgument(Description =
                "A model able to handle all the market observables required to calculate the cashflows in the portfolio.")]
            NumeraireSimulator model,
            [ExcelArgument(Description = "The number of simulations required.")]
            int nSims)
        {
            var coordinator = new Coordinator(model, new List<Simulator>(), nSims);
            return coordinator.EPE(products, valueDate, forwardValueDates);
        }


        [QuantSAExcelFunction(Description = "An approximate PFE for a portfolio of trades.",
            Name = "QSA.PFE",
            HasGeneratedVersion = true,
            Category = "QSA.Valuation",
            ExampleSheet = "PFE.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/PFE.html")]
        public static double[,] PFE([ExcelArgument(Description = "A list of products.")]
            Product[] products,
            [ExcelArgument(Description = "The value date.")]
            Date valueDate,
            [ExcelArgument(Description = "The dates at which the expected positive exposure is required.")]
            Date[] forwardValueDates,
            [ExcelArgument(Description =
                "The required percentiles.  95th percentile should be entered as 0.95.  Can be a list of percentiles and the PFE will be calculated at each of the provided levels.")]
            double[] requiredPecentiles,
            [ExcelArgument(Description =
                "A model able to handle all the market observables required to calculate the cashflows in the portfolio.")]
            NumeraireSimulator model,
            [ExcelArgument(Description = "The number of simulations required.")]
            int nSims)
        {
            var coordinator = new Coordinator(model, new List<Simulator>(), nSims);
            return coordinator.PFE(products, valueDate, forwardValueDates, requiredPecentiles);
        }
    }
}