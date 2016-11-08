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
        Category = "QSA.Equities",
            ExampleSheet = "EquityValuation.xlsx",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/CreateEquityModel.html")]
        public static object CreateEquityModel([ExcelArgument(Description = "Name of object")]string name,
        [ExcelArgument(Description = "The discounting curve.  Will be used for discounting and as the drift rate for the equities.")]object[,] discountCurve,
        [ExcelArgument(Description = "Share codes.  A list of strings to identify the shares.  These need to match those used in the product that will be valued.")]object[,] shareCodes,
        [ExcelArgument(Description = "The values of all the shares on the anchor date of the discounting curve. ")]object[,] spotPrices,
        [ExcelArgument(Description = "A single volatility for each share.")]object[,] volatilities,
        [ExcelArgument(Description = "A single continuous dividend yield rate for each equity.")]object[,] divYields,
        [ExcelArgument(Description = "A square matrix of correlations between shares, the rows and columns must be in the same order as the shares were listed in shareCodes.")]object[,] correlations,
        [ExcelArgument(Description = "The floating rate forecast curves for all the rates that the products in the portfolio will need.")]object[,] rateForecastCurves)
        {
            try
            {
                EquitySimulator simulator = new EquitySimulator(XU.GetShare1D(shareCodes, "shareCodes"),
                    XU.GetDouble1D(spotPrices, "spotPrices"), XU.GetDouble1D(volatilities, "volatilities"),
                    XU.GetDouble1D(divYields, "divYields"), XU.GetDouble2D(correlations, "correlations"),
                    XU.GetObject0D<IDiscountingSource>(discountCurve, "discountCurve"),
                    XU.GetObject1D<IFloatingRateSource>(rateForecastCurves, "rateForecastCurves"));                
                return XU.AddObject(name, simulator);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "Create a model that simulates multiple equites in one currency.  Assumes lognormal dynamics.",
            Name = "QSA.CreateEquityCall",
            Category = "QSA.Equities",
            ExampleSheet = "EquityValuation.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateEquityCall.html")]
        public static object CreateEquityCall([ExcelArgument(Description = "Name of option.")]string name,            
            [ExcelArgument(Description = "Share codes.  A list of strings to identify the shares.  These need to match those used in the product that will be valued.")]object[,] shareCode,
            [ExcelArgument(Description = "Exercise date.")]object[,] exerciseDate,
            [ExcelArgument(Description = "Strike")]object[,] strike)
                    {
            try
            {
                Product call = new EuropeanOption(XU.GetShare0D(shareCode, "shareCode"), XU.GetDouble0D(strike, "strike"), 
                    XU.GetDate0D(exerciseDate, "exerciseDate"));
                return XU.AddObject(name, call);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }
    }
}