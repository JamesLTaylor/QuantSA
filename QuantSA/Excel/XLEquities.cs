using ExcelDna.Integration;
using MonteCarlo;
using QuantSA.General;
using QuantSA.MonteCarlo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        HelpTopic = "http://cogn.co.za/QuantSA/CreateEquityModel.html")]
        public static object CreateEquityModel([ExcelArgument(Description = "Name of object")]string name,
        [ExcelArgument(Description = "The discounting curve.  Will be used for discounting and as the drift rate for the equities.")]object[,] discountCurve,
        [ExcelArgument(Description = "Share codes.  A list of strings to identify the shares.  These need to match those used in the product that will be valued.")]object[,] shareCodes,
        [ExcelArgument(Description = "The values of all the shares on the anchor date of the discounting curve. ")]object[,] spotPrices,
        [ExcelArgument(Description = "A single volatility for each share.")]object[,] volatilities,
        [ExcelArgument(Description = "A single continuous dividend yield rate for each equity.")]object[,] divYields,
        [ExcelArgument(Description = "A square matrix of correlations between shares, the rows and columns must be in the same order as the shares were listed in shareCodes.")]object[,] correlations)
        {
            try
            {
                EquitySimulator simulator = new EquitySimulator(XU.GetShares1D(shareCodes, "shareCodes"),
                    XU.GetDoubles1D(spotPrices, "spotPrices"), XU.GetDoubles1D(volatilities, "volatilities"),
                    XU.GetDoubles1D(divYields, "divYields"), XU.GetDoubles2D(correlations, "correlations"),
                    XU.GetObjects0D<IDiscountingSource>(discountCurve, "discountCurve"));                
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
            HelpTopic = "http://cogn.co.za/QuantSA/CreateEquityCall.html")]
        public static object CreateEquityCall([ExcelArgument(Description = "Name of options")]string name,            
            [ExcelArgument(Description = "Share codes.  A list of strings to identify the shares.  These need to match those used in the product that will be valued.")]object[,] shareCode,
            [ExcelArgument(Description = "Exercise date.")]object[,] exerciseDate,
            [ExcelArgument(Description = "Strike")]object[,] strike)
                    {
            try
            {
                Product call = new EuropeanOption(XU.GetShares0D(shareCode, "shareCode"), XU.GetDoubles0D(strike, "strike"), 
                    XU.GetDates0D(exerciseDate, "exerciseDate"));
                return XU.AddObject(name, call);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }
    }
}