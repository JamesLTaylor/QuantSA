using ExcelDna.Integration;
using MonteCarlo;
using QuantSA.MonteCarlo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLRates
    {
        [QuantSAExcelFunction(Description = "Create a general fixed leg of a swap.",
        Name = "QSA.CreateFixedLeg",
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/CreateFixedLeg.html")]
        public static object CreateFixedLeg([ExcelArgument(Description = "Name of object")]String name,
        [ExcelArgument(Description = "The currency of the cashflows.")]object[,] currency,
        [ExcelArgument(Description = "The dates on which the payments are made.")]object[,] paymentDates,
        [ExcelArgument(Description = "The notionals on which the payments are based.")]object[,] notionals,
        [ExcelArgument(Description = "The simple rates that are paid at each payment date.")]object[,] rates,
        [ExcelArgument(Description = "The accrual fraction to be used in calulating the fixed flow.  Will depend on the daycount convention agreed in the contract.")]object[,] accrualFractions)
        {
            try
            {
                FixedLeg fixedLeg = new FixedLeg(XU.GetCurrencies0D(currency, "currency"), XU.GetDates1D(paymentDates, "paymentDates"), 
                    XU.GetDoubles1D(notionals, "notionals"), XU.GetDoubles1D(rates, "rates"), XU.GetDoubles1D(accrualFractions, "accrualFractions")); 
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
        HelpTopic = "http://cogn.co.za/QuantSA/CreateFloatLeg.html")]
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
                FloatingIndex index = XU.GetFloatingIndices0D(floatingIndex, "floatingIndex");
                FloatingIndex[] floatingIndices = Enumerable.Range(1, resetDates.Length).Select(i => index).ToArray();
                FloatLeg floatLeg = new FloatLeg(XU.GetCurrencies0D(currency, "currency"), XU.GetDates1D(paymentDates, "paymentDates"),
                    XU.GetDoubles1D(notionals, "notionals"), XU.GetDates1D(resetDates, "resetDates"), 
                    floatingIndices, XU.GetDoubles1D(spreads, "spreads"), XU.GetDoubles1D(accrualFractions, "accrualFractions"));
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
        HelpTopic = "http://cogn.co.za/QuantSA/CreateCashLeg.html")]
        public static object CreateCashLeg([ExcelArgument(Description = "Name of object")]String name,
            [ExcelArgument(Description = "The dates on which the cashflows take place.")]object[,] paymentDates,
            [ExcelArgument(Description = "The sizes of the cashflows.  Positive for cashflows that are received.")]object[,] amounts,
            [ExcelArgument(Description = "The currencies of the cashflows. (Currency)")]object[,] currencies)
        {
            try
            {
                CashLeg cashLeg = new CashLeg(XU.GetDates1D(paymentDates, "paymentDates"), XU.GetDoubles1D(amounts, "amounts"),
                    XU.GetCurrencies1D(currencies, "currencies"));
                return XU.AddObject(name, cashLeg);
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }



        [QuantSAExcelFunction(Description = "Create a ZAR quarterly, fixed for float Jibar swap.",
        Name = "QSA.CreateZARSwap",
        Category = "QSA.Rates",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/CreateZARSwap.html")]
        public static object CreateZARSwap([ExcelArgument(Description = "Name of object")]String name,
        [ExcelArgument(Description = "First reset date of the swap")]object[,] startDate,
        [ExcelArgument(Description = "Tenor of swap, must be a whole number of years.  Example '5Y'.")]object[,] tenor,
        [ExcelArgument(Description = "The fixed rate paid or received")]double rate,
        [ExcelArgument(Description = "Is the fixed rate paid? Enter 'TRUE' for yes.")]object payFixed,
        [ExcelArgument(Description = "Flat notional for all dates.")]double notional)
        {
            try
            {

                IRSwap swap = IRSwap.CreateZARSwap(rate, XU.GetBool(payFixed), notional,
                    XU.GetDates0D(startDate, "startDate"), XU.GetTenors0D(tenor, "tenor"));
                return XU.AddObject(name, swap);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "Basic swap valuation.  Uses the same curve for forecasting and discounting and uses the 3 month rate off the curve as the Jibar Fix.",
        Name = "QSA.ValueZARSwap",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/ValueZARSwap.html")]
        public static object ValueZARSwap1Curve([ExcelArgument(Description = "Swap")]String swap,
            [ExcelArgument(Description = "The date on which valuation is required.  Cannot be before the anchor date of the curve.")]object[,] valueDate,
            [ExcelArgument(Description = "The discounting curve.  Will also be used for forecasting Jibar and providing the most recent required Jibar fix.")]string curve)
        {
            try
            {
                // Get the required objects off the map                
                Date dValueDate = XU.GetDates0D(valueDate, "valueDate");
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
                    new FloatingRateFixingCurve1Rate(rate, FloatingIndex.JIBAR3M));
                DeterminsiticCurves curveSim = new DeterminsiticCurves(discountCurve);
                curveSim.AddRateForecast(forecastCurve);
                Coordinator coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);

                // Run the valuation
                double value = coordinator.Value(new List<Product> { swapObj }, dValueDate);
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
            HelpTopic = "http://cogn.co.za/QuantSA/CreateRateForecastCurveFromDiscount.html")]
        public static object CreateRateForecastCurveFromDiscount([ExcelArgument(Description = "name")]string name,
            [ExcelArgument(Description = "The floating rate that this curve will be used to forecast. (FloatingIndex)")]object[,] floatingRateIndex,
            [ExcelArgument(Description = "The name of the discount curve that will be used to obtain the forward rates.")]object[,] discountCurve,
            [ExcelArgument(Description = "Optional: The name of the fixing curve for providing floating rates at dates before the anchor date of the discount curve.  If it is left out then the first floating rate implied by the discount curve will be used for all historical fixes.")]object[,] fixingCurve)
        {
            try
            {
                FloatingIndex index = XU.GetFloatingIndices0D(floatingRateIndex, "floatingRateIndex");
                IDiscountingSource discountCurveObj = XU.GetObjects0D<IDiscountingSource>(discountCurve, "discountCurve");

                if (!(fixingCurve[0,0] is ExcelMissing)){
                    throw new ArgumentException("fixingCurve must be lefy blank for now.  Later versions of QuantSA will allow explicit fixings to be set");
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
        HelpTopic = "http://cogn.co.za/QuantSA/ValueZARSwap.html")]
        public static object GetDF([ExcelArgument(Description = "The curve from which the DF is required.")]String curve,
            [ExcelArgument(Description = "The date on which the discount factor is required.  Cannot be before the anchor date of the curve.")]object[,] date)
        {
            try
            {
                IDiscountingSource discountCurve = ObjectMap.Instance.GetObjectFromID<IDiscountingSource>(curve);
                return discountCurve.GetDF(XU.GetDates0D(date, "date"));
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }
    }
}
