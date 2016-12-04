using ExcelDna.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using XU = QuantSA.Excel.ExcelUtilities;
using QuantSA.General;
using QuantSA.Valuation;

namespace QuantSA.Excel
{
    public class XLRates
    {
        [QuantSAExcelFunction(Description = "Create a general fixed leg of a swap.",
        Name = "QSA.CreateFixedLeg",
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/CreateFixedLeg.html")]
        public static object CreateFixedLeg([ExcelArgument(Description = "Name of object")]String name,
        [ExcelArgument(Description = "The currency of the cashflows.")]object[,] currency,
        [ExcelArgument(Description = "The dates on which the payments are made.")]object[,] paymentDates,
        [ExcelArgument(Description = "The notionals on which the payments are based.")]object[,] notionals,
        [ExcelArgument(Description = "The simple rates that are paid at each payment date.")]object[,] rates,
        [ExcelArgument(Description = "The accrual fraction to be used in calulating the fixed flow.  Will depend on the daycount convention agreed in the contract.")]object[,] accrualFractions)
        {
            try
            {
                FixedLeg fixedLeg = new FixedLeg(XU.GetCurrency0D(currency, "currency"), XU.GetDate1D(paymentDates, "paymentDates"), 
                    XU.GetDouble1D(notionals, "notionals"), XU.GetDouble1D(rates, "rates"), XU.GetDouble1D(accrualFractions, "accrualFractions")); 
                return ObjectMap.Instance.AddObject(name, fixedLeg);
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "Create a general floating leg of a swap.",
        Name = "QSA.CreateFloatLeg",
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/CreateFloatLeg.html")]
        public static object CreateFloatLeg([ExcelArgument(Description = "Name of object")]String name,
        [ExcelArgument(Description = "The currency of the cashflows. (Currency)")]object[,] currency,
        [ExcelArgument(Description = "A string describing the floating index. (FloatingIndex)")]object[,] floatingIndex,
        [ExcelArgument(Description = "The dates on which the floating indices reset.")]object[,] resetDates,
        [ExcelArgument(Description = "The dates on which the payments are made.")]object[,] paymentDates,
        [ExcelArgument(Description = "The notionals on which the payments are based.")]object[,] notionals,
        [ExcelArgument(Description = "The spreads that apply to the simple floating rates on each of the payment dates.")]object[,] spreads,
        [ExcelArgument(Description = "The accrual fraction to be used in calulating the fixed flow.  Will depend on the daycount convention agreed in the contract.")]object[,] accrualFractions)
        {
            try
            {
                FloatingIndex index = XU.GetFloatingIndex0D(floatingIndex, "floatingIndex");
                FloatingIndex[] floatingIndices = Enumerable.Range(1, resetDates.Length).Select(i => index).ToArray();
                FloatLeg floatLeg = new FloatLeg(XU.GetCurrency0D(currency, "currency"), XU.GetDate1D(paymentDates, "paymentDates"),
                    XU.GetDouble1D(notionals, "notionals"), XU.GetDate1D(resetDates, "resetDates"), 
                    floatingIndices, XU.GetDouble1D(spreads, "spreads"), XU.GetDouble1D(accrualFractions, "accrualFractions"));
                return ObjectMap.Instance.AddObject(name, floatLeg);
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "Create a general set of cashflows that can be valued like any other product.",
        Name = "QSA.CreateCashLeg",
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/CreateCashLeg.html")]
        public static object CreateCashLeg([ExcelArgument(Description = "Name of object")]String name,
            [ExcelArgument(Description = "The dates on which the cashflows take place.")]object[,] paymentDates,
            [ExcelArgument(Description = "The sizes of the cashflows.  Positive for cashflows that are received.")]object[,] amounts,
            [ExcelArgument(Description = "The currencies of the cashflows. (Currency)")]object[,] currencies)
        {
            try
            {
                CashLeg cashLeg = new CashLeg(XU.GetDate1D(paymentDates, "paymentDates"), XU.GetDouble1D(amounts, "amounts"),
                    XU.GetCurrency1D(currencies, "currencies"));
                return XU.AddObject(name, cashLeg);
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "Create a ZAR Bermudan swaption based a ZAR quarterly, fixed for float Jibar swap.",
            Name = "QSA.CreateZARBermudanSwaption",
            HasGeneratedVersion = true,
            Category = "QSA.Rates",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateZARBermudanSwaption.html")]
        public static object CreateZARBermudanSwaption([ExcelArgument(Description = "The exercise dates.  The dates on which the person who is long optionality can exercise.")]Date[] exerciseDates,
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
            Category = "QSA.Rates",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateZARSwap.html")]
        public static object CreateZARSwap([ExcelArgument(Description = "First reset date of the swap")]Date startDate,
        [ExcelArgument(Description = "Tenor of swap, must be a whole number of years.  Example '5Y'.")]Tenor tenor,
        [ExcelArgument(Description = "The fixed rate paid or received")]double rate,
        [ExcelArgument(Description = "Is the fixed rate paid? Enter 'TRUE' for yes.")]bool payFixed,
        [ExcelArgument(Description = "Flat notional for all dates.")]double notional)
        {
            return IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);
        }

        [QuantSAExcelFunction(Description = "Basic swap valuation.  Uses the same curve for forecasting and discounting and uses the 3 month rate off the curve as the Jibar Fix.",
        Name = "QSA.ValueZARSwap",
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/ValueZARSwap.html")]
        public static object ValueZARSwap1Curve([ExcelArgument(Description = "The name of the swap.")]String swap,
            [ExcelArgument(Description = "The date on which valuation is required.  Cannot be before the anchor date of the curve.")]object[,] valueDate,
            [ExcelArgument(Description = "The discounting curve.  Will also be used for forecasting Jibar and providing the most recent required Jibar fix.")]string curve)
        {
            try
            {
                // Get the required objects off the map                
                Date dValueDate = XU.GetDate0D(valueDate, "valueDate");
                IRSwap swapObj = ObjectMap.Instance.GetObjectFromID<IRSwap>(swap);
                IDiscountingSource discountCurve = ObjectMap.Instance.GetObjectFromID<IDiscountingSource>(curve);
                FloatingIndex index = swapObj.GetFloatingIndex();

                // Calculate the first fixing off the curve to use at all past dates.
                double df1 = discountCurve.GetDF(dValueDate);
                Date laterDate = dValueDate.AddTenor(index.tenor);
                double df2 = discountCurve.GetDF(laterDate);
                double dt = (laterDate - dValueDate) / 365.0;
                double rate = (df1 / df2 - 1) / dt;

                //Set up the valuation engine.
                IFloatingRateSource forecastCurve = new ForecastCurveFromDiscount(discountCurve, index,
                    new FloatingRateFixingCurve1Rate(rate, index));
                DeterminsiticCurves curveSim = new DeterminsiticCurves(discountCurve);
                curveSim.AddRateForecast(forecastCurve);
                Coordinator coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);

                // Run the valuation
                double value = coordinator.Value(new Product[] { swapObj }, dValueDate);
                return value;
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Description = "Create a curve to forecast floating interest rates based on a discount curve.",
            Name = "QSA.CreateRateForecastCurveFromDiscount",
            Category = "QSA.Rates",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateRateForecastCurveFromDiscount.html")]
        public static object CreateRateForecastCurveFromDiscount([ExcelArgument(Description = "The name of the new curve.")]string name,
            [ExcelArgument(Description = "The floating rate that this curve will be used to forecast. (FloatingIndex)")]object[,] floatingRateIndex,
            [ExcelArgument(Description = "The name of the discount curve that will be used to obtain the forward rates.")]object[,] discountCurve,
            [ExcelArgument(Description = "Optional: The name of the fixing curve for providing floating rates at dates before the anchor date of the discount curve.  If it is left out then the first floating rate implied by the discount curve will be used for all historical fixes.")]object[,] fixingCurve)
        {
            try
            {
                FloatingIndex index = XU.GetFloatingIndex0D(floatingRateIndex, "floatingRateIndex");
                IDiscountingSource discountCurveObj = XU.GetObject0D<IDiscountingSource>(discountCurve, "discountCurve");

                if (!(fixingCurve[0,0] is ExcelMissing)){
                    throw new ArgumentException("fixingCurve must be left blank for now.  Later versions of QuantSA will allow explicit fixings to be set");
                }

                // Calculate the first fixing off the curve to use at all past dates.
                double df1 = 1.0;
                Date laterDate = discountCurveObj.getAnchorDate().AddTenor(index.tenor);
                double df2 = discountCurveObj.GetDF(laterDate);
                double dt = (laterDate - discountCurveObj.getAnchorDate()) / 365.0;
                double rate = (df1 / df2 - 1) / dt;

                ForecastCurveFromDiscount forecastCurve = new ForecastCurveFromDiscount(discountCurveObj, index,
                    new FloatingRateFixingCurve1Rate(rate, index));

                return XU.AddObject(name, forecastCurve);                
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Description = "Get the discount factor from a curve object.  The DF will be from the anchor date until the supplied date.",
        Name = "QSA.GetDF",
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/GetDF.html")]
        public static object GetDF([ExcelArgument(Description = "The curve from which the DF is required.")]String curve,
            [ExcelArgument(Description = "The date on which the discount factor is required.  Cannot be before the anchor date of the curve.")]object[,] date)
        {
            try
            {
                IDiscountingSource discountCurve = ObjectMap.Instance.GetObjectFromID<IDiscountingSource>(curve);
                return discountCurve.GetDF(XU.GetDate0D(date, "date"));
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "Create fixed rate loan.",
        Name = "QSA.CreateLoanFixedRate",
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/CreateLoanFixedRate.html",
            ExampleSheet ="Loans.xlsx")]
        public static object CreateLoanFixedRate([ExcelArgument(Description = "Name of object")]string name,
        [ExcelArgument(Description = "The currency of the cashflows.")]object[,] currency,
        [ExcelArgument(Description = "The dates on which the loan balances are known.  All dates other than the first one will be assumed to also be cashflow dates.")]object[,] balanceDates,
        [ExcelArgument(Description = "The notionals on which the payments are based.")]object[,] balanceAmounts,
        [ExcelArgument(Description = "The simple rates that are paid at each payment date.")]object[,] fixedRate)
        {
            try
            {
                LoanFixedRate loan = LoanFixedRate.CreateSimple(XU.GetDate1D(balanceDates, "balanceDates"),
                    XU.GetDouble1D(balanceAmounts, "balanceAmounts"), XU.GetDouble0D(fixedRate, "fixedRate"),
                    XU.GetCurrency0D(currency, "currency"));
                return XU.AddObject(name, loan);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Description = "Create floationg rate loan.",
        Name = "QSA.CreateLoanFloatingRate",
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/CreateLoanFloatingRate.html",
            ExampleSheet = "Loans.xlsx")]
        public static object CreateLoanFloatingRate([ExcelArgument(Description = "Name of object")]string name,
        [ExcelArgument(Description = "The currency of the cashflows.")]object[,] currency,
        [ExcelArgument(Description = "The dates on which the loan balances are known.  All dates other than the first one will be assumed to also be cashflow dates.")]object[,] balanceDates,
        [ExcelArgument(Description = "The notionals on which the payments are based.")]object[,] balanceAmounts,
        [ExcelArgument(Description = "The reference index on which the floating flows are based.")]object[,] floatingIndex,
        [ExcelArgument(Description = "The spread that will be added to the floating index.")]object[,] floatingSpread)
        {
            try
            {
                LoanFloatingRate loan = LoanFloatingRate.CreateSimple(XU.GetDate1D(balanceDates, "balanceDates"),
                    XU.GetDouble1D(balanceAmounts, "balanceAmounts"), XU.GetFloatingIndex0D(floatingIndex, "floatingIndex"),
                    XU.GetDouble0D(floatingSpread, "floatingSpread"), XU.GetCurrency0D(currency, "currency"));
                return XU.AddObject(name, loan);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "Create demo Hull White model.  Will be used for discounting and forecasting any indices specified.",
        Name = "QSA.CreateHWModelDemo",
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/CreateHWModelDemo.html",
            ExampleSheet = "")]
        public static object CreateHWModelDemo([ExcelArgument(Description = "Name of object")]string name,            
            [ExcelArgument(Description = "")]object[,] meanReversion,
            [ExcelArgument(Description = "")]object[,] flatVol,
            [ExcelArgument(Description = "")]object[,] baseCurve,
            [ExcelArgument(Description = "")]object[,] forecastIndices)
        {
            try
            {
                double a = XU.GetDouble0D(meanReversion, "meanReversion");
                double vol = XU.GetDouble0D(flatVol, "flatVol");
                IDiscountingSource curve = XU.GetObject0D<IDiscountingSource>(baseCurve, "baseCurve");
                Date anchorDate = curve.getAnchorDate();
                double flatCurveRate = -Math.Log(curve.GetDF(anchorDate.AddTenor(Tenor.Years(1))));

                HullWhite1F hullWiteSim = new HullWhite1F(a, vol, flatCurveRate, flatCurveRate, anchorDate);                
                hullWiteSim.AddForecast(XU.GetFloatingIndex0D(forecastIndices, "forecastIndices"));                
                return XU.AddObject(name, hullWiteSim);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

    }
}
