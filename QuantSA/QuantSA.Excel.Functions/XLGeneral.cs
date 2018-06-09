using ExcelDna.Integration;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Excel.Shared;
using QuantSA.General;
using QuantSA.General.Conventions.DayCount;
using QuantSA.General.Formulae;
using QuantSA.Primitives.Dates;

namespace QuantSA.ExcelFunctions
{
    public class XLGeneral
    {
        [QuantSAExcelFunction(Description = "The Black Scholes formula for a call.",
            IsHidden = false,
            Name = "QSA.FormulaBlackScholes",
            ExampleSheet = "EquityValuation.xlsx",
            Category = "QSA.General",
            HelpTopic = "http://www.quantsa.org/FormulaBlackScholes.html")]
        public static double FormulaBlackScholes([ExcelArgument(Description = "Strike")]
            double strike,
            [ExcelArgument(Description = "The value date as and Excel date.")]
            Date valueDate,
            [ExcelArgument(Description = "The exercise date of the option.  Must be greater than the value date.")]
            Date exerciseDate,
            [ExcelArgument(Description = "The spot price of the underlying at the value date.")]
            double spotPrice,
            [ExcelArgument(Description = "Annualized volatility.")]
            double vol,
            [ExcelArgument(Description = "Continuously compounded risk free rate.")]
            double riskfreeRate,
            [QuantSAExcelArgument(Description = "Continuously compounded dividend yield.", Default = "0.0")]
            double divYield)
        {
            return BlackEtc.BlackScholes(PutOrCall.Call, strike,
                Actual365Fixed.Instance.YearFraction(valueDate, exerciseDate),
                spotPrice, vol, riskfreeRate, divYield);
        }


        [QuantSAExcelFunction(Description = "Create a product defined in a script file.",
            IsHidden = false,
            Name = "QSA.CreateProductFromFile",
            Category = "QSA.General",
            ExampleSheet = "CreateProductFromFile.xlsx",
            HelpTopic = "http://www.quantsa.org/CreateProductFromFile.html")]
        public static Product CreateProductFromFile([ExcelArgument(Description = "Full path to the file.")]
            string filename)
        {
            return RuntimeProduct.CreateFromScript(filename);
        }


        [QuantSAExcelFunction(Description = "A linear interpolator.",
            IsHidden = false,
            Name = "QSA.InterpLinear",
            ExampleSheet = "InterpLinear.xlsx",
            Category = "QSA.General",
            HelpTopic = "http://www.quantsa.org/InterpLinear.html")]
        public static double[,] InterpLinear(
            [ExcelArgument(Description = "A vector of x values.  Must be in increasing order")]
            double[] knownX,
            [ExcelArgument(Description = "A vector of y values.  Must be the same length as knownX")]
            double[] knownY,
            [ExcelArgument(Description = "x values at which interpolation is required.")]
            double[,] requiredX)
        {
            var curve = new InterpolatedCurve(knownX, knownY);
            return curve.Interp(requiredX);
        }
    }
}