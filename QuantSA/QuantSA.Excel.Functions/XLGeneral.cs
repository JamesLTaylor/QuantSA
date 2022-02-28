using ExcelDna.Integration;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Formulae;
using QuantSA.Core.Primitives;
using QuantSA.Core.Products;
using QuantSA.Core.Products.SAMarket;
using QuantSA.CoreExtensions.SAMarket;
using QuantSA.Excel.Shared;
using QuantSA.Shared.Conventions.DayCount;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;
using System;

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
        public static double FormulaBlackScholes([QuantSAExcelArgument(Description = "PutOrCall")] PutOrCall putOrCall, [ExcelArgument(Description = "Strike")]
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
            return BlackEtc.BlackScholes(putOrCall, strike,
                Actual365Fixed.Instance.YearFraction(valueDate, exerciseDate),
                spotPrice, vol, riskfreeRate, divYield);
        }

        [QuantSAExcelFunction(Description = "The Black Scholes formula for a call.",
            IsHidden = false,
            Name = "QSA.FormulaBlack",
            ExampleSheet = "CapletValuation.xlsx",
            Category = "QSA.General",
            HelpTopic = "http://www.quantsa.org/FormulaBlack.html")]
        public static double FormulaBlack([ExcelArgument(Description = "put or call.")]
            PutOrCall putOrCall,
            [ExcelArgument(Description = "The strike")]
            double strike,
            [ExcelArgument(Description = "The time to maturity in years from the value date to the exercise date.")]
            double timeToExercise,
            [ExcelArgument(Description = "The forward at the option exercise date.")]
            double forward,
            [ExcelArgument(Description = "Annualized volatility.")]
            double vol,
            [ExcelArgument(Description = "The discount factor from the value date to the settlement date of the option.")]
            double discountFactor)
        {
            return BlackEtc.Black(putOrCall, strike, timeToExercise, forward, vol, discountFactor);
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

        [QuantSAExcelFunction(
                    Description = "Create a Besa JSE Bond.",
                    Name = "QSA.CreateBesaJSEBond",
                    HasGeneratedVersion = true,
                    ExampleSheet = "BesaJSEBond.xlsx",
                    Category = "QSA.General",
                    IsHidden = false,
                    HelpTopic = "")]
        public static BesaJseBond CreateBesaJseBond(
            [ExcelArgument(Description = "The maturity date of the bond.")]
            Date maturityDate,
            [ExcelArgument(Description = "The notional amount of the bond.")]
            double notional,
            [ExcelArgument(Description = "The annual coupon rate of the bond.")]
            double annualCouponRate,
            [ExcelArgument(Description = "The month the first bond coupon is paid.")]
            int couponMonth1,
            [ExcelArgument(Description = "The day the first bond coupon is paid.")]
            int couponDay1,
            [ExcelArgument(Description = "The month the second bond coupon is paid.")]
            int couponMonth2,
            [ExcelArgument(Description = "The day the second bond coupon is paid.")]
            int couponDay2,
            [ExcelArgument(Description = "The books close date days of the bond.")]
            int bookscloseDateDays,
            [ExcelArgument(Description = "The currency of the cashflows.")]
            Currency currency)

        {
            return new BesaJseBond(maturityDate, notional, annualCouponRate, couponMonth1, couponDay1, couponMonth2, couponDay2, bookscloseDateDays, new Calendar("ZA"), currency);
        }

        [QuantSAExcelFunction(
            Description = "Returns all key output parameters of a Besa JSE Bond.",
            Name = "QSA.BesaJseBondResults",
            HasGeneratedVersion = true,
            ExampleSheet = "BesaJSEBond.xlsx",
            Category = "QSA.General",
            IsHidden = false,
            HelpTopic = "")]

        public static string[,] BesaJseBondResults(
            [ExcelArgument(Description = "The underlying bond.")]
            BesaJseBond besaJseBond,
            [ExcelArgument(Description = "The settlement date of the bond.")]
            Date settleDate,
            [ExcelArgument(Description = "The yield to maturity of the bond.")]
            double ytm)

        {
            var results = besaJseBond.GetSpotMeasures(settleDate, ytm);
            string[,] measures = { {"roundedAip", results.GetScalar(BesaJseBondEx.Keys.RoundedAip).ToString()}, {"unroundedAip", results.GetScalar(BesaJseBondEx.Keys.UnroundedAip).ToString()},
                {"roundedClean", results.GetScalar(BesaJseBondEx.Keys.RoundedClean).ToString()},{"unroundedClean", results.GetScalar(BesaJseBondEx.Keys.UnroundedClean).ToString()},
                {"unroundedAccrued", results.GetScalar(BesaJseBondEx.Keys.UnroundedAccrued).ToString()} };

            return measures;
        }

        [QuantSAExcelFunction(
            Description = "Create a Besa JSE Bond Forward.",
            Name = "QSA.CreateBesaJSEBondForward",
            HasGeneratedVersion = true,
            ExampleSheet = "BesaJSEBondForward.xlsx",
            Category = "QSA.General",
            IsHidden = false,
            HelpTopic = "")]
        public static JSEBondForward CreateBesaJseBondForward(
            [ExcelArgument(Description = "The maturity date of the bond.")]
                    Date maturity,
            [ExcelArgument(Description = "The forward date of the contract.")]
                    Date forwardDate,
            [ExcelArgument(Description = "The notional amount of the bond.")]
                    double notional,
            [ExcelArgument(Description = "The annual coupon rate of the bond.")]
                    double annualCouponRate,
            [ExcelArgument(Description = "The month the first bond coupon is paid.")]
                    int couponMonth1,
            [ExcelArgument(Description = "The day the first bond coupon is paid.")]
                    int couponDay1,
            [ExcelArgument(Description = "The month the second bond coupon is paid.")]
                    int couponMonth2,
            [ExcelArgument(Description = "The day the second bond coupon is paid.")]
                    int couponDay2,
            [ExcelArgument(Description = "The books close date days of the bond.")]
                    int bookscloseDateDays,
            [ExcelArgument(Description = "The currency of the cashflows.")]
                    Currency currency)

        {
            return new JSEBondForward(forwardDate, maturity, notional, annualCouponRate, couponMonth1, couponDay1, couponMonth2, couponDay2, bookscloseDateDays, new Calendar("ZA"), currency);
        }

        [QuantSAExcelFunction(
            Description = "Returns the forward price of a Besa JSE Bond Forward.",
            Name = "QSA.BesaJseBondForwardPrice",
            HasGeneratedVersion = true,
            ExampleSheet = "BesaJSEBondForward.xlsx",
            Category = "QSA.General",
            IsHidden = false,
            HelpTopic = "")]

        public static double BesaJseForwardPrice(
            [ExcelArgument(Description = "The underlying bond.")]
                    JSEBondForward bondforward,
            [ExcelArgument(Description = "The settlement date of the bond.")]
                    Date settleDate,
            [ExcelArgument(Description = "The yield to maturity of the bond.")]
                    double ytm,
            [ExcelArgument(Description = "The repo rate of the bond.")]
                double repo)

        {
            var result = (double)bondforward.ForwardPrice(settleDate, ytm, repo).GetScalar(JSEBondForwardEx.Keys.ForwardPrice);
            return result;
        }

        [QuantSAExcelFunction(
            Description = "Create a Besa JSE Bond Option.",
            Name = "QSA.CreateBesaJSEBondOption",
            HasGeneratedVersion = true,
            ExampleSheet = "BesaJSEBondOption.xlsx",
            Category = "QSA.General",
            IsHidden = false,
            HelpTopic = "")]
        public static JSEBondOption CreateBesaJseBondOption(
            [ExcelArgument(Description = "The forward date of the bond.")]
                            Date forwardDate,
            [ExcelArgument(Description = "The maturity date of the contract.")]
                            Date maturityDate,
            [ExcelArgument(Description = "put or call.")]
                            PutOrCall putOrCall,
            [ExcelArgument(Description = "The settlement date of the bond.")]
                            Date settleDate)
        {
            return new JSEBondOption(forwardDate, maturityDate, putOrCall, settleDate);
        }

        [QuantSAExcelFunction(Description = "Returns the option price of a Besa JSE Bond Option.",
            IsHidden = false,
            Name = "QSA.BesaJseBondOptionPrice",
            ExampleSheet = "BesaJSEBondOption.xlsx",
            Category = "QSA.General")]
        // HelpTopic = "http://www.quantsa.org/")] // TODO:
        public static double FormulaBondOption([ExcelArgument(Description = "The underlying bond option.")]
                JSEBondOption bondOptionR153,
                [ExcelArgument(Description = "The strike/struck rate of the option.")]
                        double strike,
                [ExcelArgument(Description = "Annualized volatility.")]
                        double vol,
                [ExcelArgument(Description = "The repo rate of the deal.")]
                        double repo,
                [ExcelArgument(Description = "The yield to maturity of the bond.")]
                        double ytm,
                [ExcelArgument(Description = "The underlying bond forward from which the forward price is derived.")]
                        JSEBondForward bondForward)

        {
            var bondOptionPrice = (double)JSEBondOptionEx.BlackOption(bondOptionR153, strike, vol, repo, bondForward, ytm).GetScalar(JSEBondOptionEx.Keys.BlackOption);
            return bondOptionPrice;
        }

        [QuantSAExcelFunction(Description = "The discount factor of a bond option for testing purposes.",
            IsHidden = false,
            Name = "QSA.FormulaDF",
            ExampleSheet = "BesaJSEBondOption.xlsx",
            Category = "QSA.General")]
        // HelpTopic = "http://www.quantsa.org/")] // TODO:
        public static double FormulaDF(
            [ExcelArgument(Description = "The time to maturity/exercise.")]
                    double timeToMaturity,
            [ExcelArgument(Description = "The repo rate of the bond.")]
                double repo)

        {
            var discountFactor = Math.Exp(-repo * timeToMaturity);
            return discountFactor;
        }
    }
}