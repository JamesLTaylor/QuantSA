using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using QuantSA.General;
using QuantSA.Valuation;
using QuantSA.Excel.Common;

namespace QuantSA.ExcelFunctions
{
    public class XLRates
    {
        [QuantSAExcelFunction(Description = "Create a general fixed leg of a swap.",
            Name = "QSA.CreateFixedLeg",
            HasGeneratedVersion = true,
            ExampleSheet = "GeneralSwap.xlsx",
            Category = "QSA.Rates",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateFixedLeg.html")]
        public static FixedLeg CreateFixedLeg([ExcelArgument(Description = "The currency of the cashflows.")]Currency currency,
        [ExcelArgument(Description = "The dates on which the payments are made.")]Date[] paymentDates,
        [ExcelArgument(Description = "The notionals on which the payments are based.")]double[] notionals,
        [ExcelArgument(Description = "The simple rates that are paid at each payment date.")]double[] rates,
        [ExcelArgument(Description = "The accrual fraction to be used in calulating the fixed flow.  Will depend on the daycount convention agreed in the contract.")]double[] accrualFractions)
        {
            return new FixedLeg(currency, paymentDates, notionals, rates, accrualFractions);
        }

        [QuantSAExcelFunction(Description = "Create a general floating leg of a swap.",
            Name = "QSA.CreateFloatLeg",
            HasGeneratedVersion =true,
            ExampleSheet = "GeneralSwap.xlsx",
            Category = "QSA.Rates",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateFloatLeg.html")]
        public static FloatLeg CreateFloatLeg([ExcelArgument(Description = "The currency of the cashflows. (Currency)")]Currency currency,
        [ExcelArgument(Description = "A string describing the floating index.")]FloatingIndex floatingIndex,
        [ExcelArgument(Description = "The dates on which the floating indices reset.")]Date[] resetDates,
        [ExcelArgument(Description = "The dates on which the payments are made.")]Date[] paymentDates,
        [ExcelArgument(Description = "The notionals on which the payments are based.")]double[] notionals,
        [ExcelArgument(Description = "The spreads that apply to the simple floating rates on each of the payment dates.")]double[] spreads,
        [ExcelArgument(Description = "The accrual fraction to be used in calulating the fixed flow.  Will depend on the daycount convention agreed in the contract.")]double[] accrualFractions)
        {
            FloatingIndex[] floatingIndices = Enumerable.Range(1, resetDates.Length).Select(i => floatingIndex).ToArray();
            return new FloatLeg(currency, paymentDates, notionals, resetDates, floatingIndices, spreads, accrualFractions);
        }


        [QuantSAExcelFunction(Description = "Create a general set of cashflows that can be valued like any other product.",
            Name = "QSA.CreateCashLeg",
            Category = "QSA.Rates",
            HasGeneratedVersion = true,
            ExampleSheet = "GeneralSwap.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateCashLeg.html")]
        public static CashLeg CreateCashLeg([ExcelArgument(Description = "The dates on which the cashflows take place.")]Date[] paymentDates,
            [ExcelArgument(Description = "The sizes of the cashflows.  Positive for cashflows that are received.")]double[] amounts,
            [ExcelArgument(Description = "The currencies of the cashflows.")]Currency[] currencies)
        {
            return new CashLeg(paymentDates, amounts, currencies);
        }

        [QuantSAExcelFunction(Description = "Create a ZAR Bermudan swaption based a ZAR quarterly, fixed for float Jibar swap.",
            Name = "QSA.CreateZARBermudanSwaption",
            HasGeneratedVersion = true,
            ExampleSheet = "BermudanSwaption.xlsx",
            Category = "QSA.Rates",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateZARBermudanSwaption.html")]
        public static BermudanSwaption CreateZARBermudanSwaption([ExcelArgument(Description = "The exercise dates.  The dates on which the person who is long optionality can exercise.")]Date[] exerciseDates,
            [ExcelArgument(Description = "if set to TRUE then the person valuing this product owns the optionality.")]bool longOptionality,
            [ExcelArgument(Description = "First reset date of the underlying swap.")]Date startDate,
            [ExcelArgument(Description = "Tenor of underlying swap, must be a whole number of years.  Example '5Y'.")]Tenor tenor,
            [ExcelArgument(Description = "The fixed rate paid or received on the underlying swap.")]double rate,
            [ExcelArgument(Description = "Is the fixed rate paid? Enter 'TRUE' for yes.")]bool payFixed,
            [ExcelArgument(Description = "Flat notional for all dates.")]double notional)
        {
            return BermudanSwaption.CreateZARBermudanSwaption(exerciseDates, longOptionality, rate, payFixed, notional, startDate, tenor);
        }


        [QuantSAExcelFunction(Description = "Create a ZAR quarterly, fixed for float Jibar swap.",
            Name = "QSA.CreateZARSwap",
            HasGeneratedVersion = true,
            ExampleSheet = "ZARSwap.xlsx", 
            Category = "QSA.Rates",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateZARSwap.html")]
        public static IRSwap CreateZARSwap([ExcelArgument(Description = "First reset date of the swap")]Date startDate,
        [ExcelArgument(Description = "Tenor of swap, must be a whole number of years.  Example '5Y'.")]Tenor tenor,
        [ExcelArgument(Description = "The fixed rate paid or received")]double rate,
        [ExcelArgument(Description = "Is the fixed rate paid? Enter 'TRUE' for yes.")]bool payFixed,
        [ExcelArgument(Description = "Flat notional for all dates.")]double notional)
        {
            return IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);
        }

        [QuantSAExcelFunction(Description = "Basic swap valuation.  Uses the same curve for forecasting and discounting and uses the 3 month rate off the curve as the Jibar Fix.",
            Name = "QSA.ValueZARSwap",
            HasGeneratedVersion = true,
            ExampleSheet = "ZARSwap.xlsx",
            Category = "QSA.Rates",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/ValueZARSwap.html")]
        public static double ValueZARSwap1Curve([ExcelArgument(Description = "The name of the swap.")]IRSwap swap,
            [ExcelArgument(Description = "The date on which valuation is required.  Cannot be before the anchor date of the curve.")]Date valueDate,
            [ExcelArgument(Description = "The discounting curve.  Will also be used for forecasting Jibar and providing the most recent required Jibar fix.")]IDiscountingSource curve)
        {
            // Get the required objects off the map                                                
            FloatingIndex index = swap.GetFloatingIndex();

            // Calculate the first fixing off the curve to use at all past dates.
            double df1 = curve.GetDF(valueDate);
            Date laterDate = valueDate.AddTenor(index.tenor);
            double df2 = curve.GetDF(laterDate);
            double dt = (laterDate - valueDate) / 365.0;
            double rate = (df1 / df2 - 1) / dt;

            //Set up the valuation engine.
            IFloatingRateSource forecastCurve = new ForecastCurveFromDiscount(curve, index,
                new FloatingRateFixingCurve1Rate(rate, index));
            DeterminsiticCurves curveSim = new DeterminsiticCurves(curve);
            curveSim.AddRateForecast(forecastCurve);
            Coordinator coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);

            // Run the valuation
            double value = coordinator.Value(new Product[] { swap }, valueDate);
            return value;
        }


        [QuantSAExcelFunction(Description = "Create a curve to forecast floating interest rates based on a discount curve.",
            Name = "QSA.CreateRateForecastCurveFromDiscount",
            HasGeneratedVersion = true,
            ExampleSheet = "GeneralSwap.xlsx",
            Category = "QSA.Rates",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateRateForecastCurveFromDiscount.html")]
        public static IFloatingRateSource CreateRateForecastCurveFromDiscount([ExcelArgument(Description = "The floating rate that this curve will be used to forecast.")]FloatingIndex floatingRateIndex,
            [ExcelArgument(Description = "The name of the discount curve that will be used to obtain the forward rates.")]IDiscountingSource discountCurve,
            [QuantSAExcelArgument(Description = "Optional: The name of the fixing curve for providing floating rates at dates before the anchor date of the discount curve.  If it is left out then the first floating rate implied by the discount curve will be used for all historical fixes.", Default="null")]IFloatingRateSource fixingCurve)
        {
            if (fixingCurve == null) {
                // Calculate the first fixing off the curve to use at all past dates.
                double df1 = 1.0;
                Date laterDate = discountCurve.GetAnchorDate().AddTenor(floatingRateIndex.tenor);
                double df2 = discountCurve.GetDF(laterDate);
                double dt = (laterDate - discountCurve.GetAnchorDate()) / 365.0;
                double rate = (df1 / df2 - 1) / dt;
                fixingCurve = new FloatingRateFixingCurve1Rate(rate, floatingRateIndex);
            }
            return new ForecastCurveFromDiscount(discountCurve, floatingRateIndex, fixingCurve);
        }


        [QuantSAExcelFunction(Description = "Get the discount factor from a curve object.  The DF will be from the anchor date until the supplied date.",
            Name = "QSA.GetDF",
            HasGeneratedVersion = true,
            Category = "QSA.Rates",
            ExampleSheet = "Introduction.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetDF.html")]
        public static double GetDF([ExcelArgument(Description = "The curve from which the DF is required.")]IDiscountingSource curve,
            [ExcelArgument(Description = "The date on which the discount factor is required.  Cannot be before the anchor date of the curve.")]Date date)
        {
            return curve.GetDF(date);            
        }


        [QuantSAExcelFunction(Description = "Create fixed rate loan.",
            Name = "QSA.CreateLoanFixedRate",
            Category = "QSA.Rates",
            HasGeneratedVersion = true,
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateLoanFixedRate.html",
            ExampleSheet ="Loans.xlsx")]
        public static LoanFixedRate CreateLoanFixedRate([ExcelArgument(Description = "The currency of the cashflows.")]Currency currency,
        [ExcelArgument(Description = "The dates on which the loan balances are known.  All dates other than the first one will be assumed to also be cashflow dates.")]Date[] balanceDates,
        [ExcelArgument(Description = "The notionals on which the payments are based.")]double[] balanceAmounts,
        [ExcelArgument(Description = "The simple rates that are paid at each payment date.")]double fixedRate)
        {
            return LoanFixedRate.CreateSimple(balanceDates, balanceAmounts, fixedRate, currency);
        }


        [QuantSAExcelFunction(Description = "Create floationg rate loan.",
        Name = "QSA.CreateLoanFloatingRate",
            HasGeneratedVersion = true,
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/CreateLoanFloatingRate.html",
            ExampleSheet = "Loans.xlsx")]
        public static LoanFloatingRate CreateLoanFloatingRate([ExcelArgument(Description = "The currency of the cashflows.")]Currency currency,
        [ExcelArgument(Description = "The dates on which the loan balances are known.  All dates other than the first one will be assumed to also be cashflow dates.")]Date[] balanceDates,
        [ExcelArgument(Description = "The notionals on which the payments are based.")]double[] balanceAmounts,
        [ExcelArgument(Description = "The reference index on which the floating flows are based.")]FloatingIndex floatingIndex,
        [ExcelArgument(Description = "The spread that will be added to the floating index.")]double floatingSpread)
        {
            return LoanFloatingRate.CreateSimple(balanceDates, balanceAmounts, floatingIndex, floatingSpread, currency);            
        }


        [QuantSAExcelFunction(Description = "Create demo Hull White model.  Will be used for discounting and forecasting any indices specified.",
            Name = "QSA.CreateHWModelDemo",
            Category = "QSA.Rates",
            HasGeneratedVersion = true,
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateHWModelDemo.html",
            ExampleSheet = "EPE.xlsx")]
        public static HullWhite1F CreateHWModelDemo([ExcelArgument(Description = "The constant rate of mean reversion.")]double meanReversion,
            [ExcelArgument(Description = "The constant short rate volatility.  Note that this is a Gaussian vol and will in general be lower than the vol that would be used in Black.")]double flatVol,
            [ExcelArgument(Description = "The curve to which zero coupon bond prices will be calibrated.")]IDiscountingSource baseCurve,
            [ExcelArgument(Description = "The indices that should be forecast with this same cuve.  No spreads are added.")]FloatingIndex forecastIndices)
        {
            Date anchorDate = baseCurve.GetAnchorDate();
            double flatCurveRate = -Math.Log(baseCurve.GetDF(anchorDate.AddTenor(Tenor.Years(1))));
            HullWhite1F model = new HullWhite1F(baseCurve.GetCurrency(), meanReversion, flatVol, flatCurveRate, flatCurveRate, anchorDate);
            model.AddForecast(forecastIndices);
            return model;
        }

    }
}
