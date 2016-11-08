using ExcelDna.Integration;
using System;
using QuantSA.General;
using QuantSA.Valuation;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLEquities
    {
        [QuantSAExcelFunction(Description = "Create a model that simulates multiple equites in one currency.  Assumes lognormal dynamics.",
            Name = "QSA.CreateEquityModel",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "EquityValuation.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateEquityModel.html")]
        public static NumeraireSimulator CreateEquityModel([ExcelArgument(Description = "The discounting curve.  Will be used for discounting and as the drift rate for the equities.")]IDiscountingSource discountCurve,
            [ExcelArgument(Description = "Share codes.  A list of strings to identify the shares.  These need to match those used in the product that will be valued.")]Share[] shares,
            [ExcelArgument(Description = "The values of all the shares on the anchor date of the discounting curve. ")]double[] spotPrices,
            [ExcelArgument(Description = "A single volatility for each share.")]double[] volatilities,
            [ExcelArgument(Description = "A single continuous dividend yield rate for each equity.")]double[] divYields,
            [ExcelArgument(Description = "A square matrix of correlations between shares, the rows and columns must be in the same order as the shares were listed in shareCodes.")]double[,] correlations,
            [ExcelArgument(Description = "The floating rate forecast curves for all the rates that the products in the portfolio will need.")]IFloatingRateSource[] rateForecastCurves)
        {
            return new EquitySimulator(shares, spotPrices, volatilities, divYields, correlations, discountCurve,
                rateForecastCurves);
        }


        [QuantSAExcelFunction(Description = "Create a model that simulates multiple equites in one currency.  Assumes lognormal dynamics.",
            Name = "QSA.CreateEquityCall",
            HasGeneratedVersion = true,
            Category = "QSA.Equities",
            ExampleSheet = "EquityValuation.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateEquityCall.html")]
        public static Product CreateEquityCall([ExcelArgument(Description = "A share.  This needs to match a share in the model that will be used to value this.")]Share share,
            [ExcelArgument(Description = "Exercise date.")]Date exerciseDate,
            [ExcelArgument(Description = "Strike")]double strike)
        {
            return new EuropeanOption(share, strike, exerciseDate);
        }
    }
}