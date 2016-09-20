using ExcelDna.Integration;
using MonteCarlo;
using QuantSA.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.Excel
{
    public class XLRates
    {
        [QuantSAExcelFunction(Description = "Create a ZAR quarterly, fixed for float Jibar swap.",
        Name = "QSA.CreateZARSwap",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/CreateZARSwap.html")]
        public static object CreateZARSwap([ExcelArgument(Description = "Name of object")]String name,
        [ExcelArgument(Description = "First reset date of the swap")]double startDate,
        [ExcelArgument(Description = "Tenor of swap, must be a whole number of years.  Example '5Y'.")]object tenor,
        [ExcelArgument(Description = "The fixed rate paid or received")]double rate,
        [ExcelArgument(Description = "Is the fixed rate paid? Enter 'TRUE' for yes.")]object payFixed,
        [ExcelArgument(Description = "Flat notional for all dates.")]double notional)
        {        
            try
            {

                IRSwap swap = IRSwap.CreateZARSwap(rate, ExcelUtilities.GetBool(payFixed), notional,
                    ExcelUtilities.GetDates(startDate), ExcelUtilities.GetTenor(tenor));
                return ObjectMap.Instance.AddObject(name, swap);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [QuantSAExcelFunction(Description = "Basic swap valuation.  Uses the same curve for forecasting and discounting and uses the 3 month rate off the curve as the Jibar Fix.",
        Name = "QSA.ValueZARSwap",
        Category = "QSA.General",
        IsHidden = false,
        HelpTopic = "http://cogn.co.za/QuantSA/ValueZARSwap.html")]
        public static object ValueZARSwap1Curve([ExcelArgument(Description = "Swap")]String swap,
            [ExcelArgument(Description = "The date on which valuation is required.  Cannot be before the anchor date of the curve.")]double valueDate,
            [ExcelArgument(Description = "The discounting curve.  Will also be used for forecasting Jibar and providing the most recent required Jibar fix.")]string curve)
        {
            try
            {
                // Get the required objects off the map
                FloatingIndex index = FloatingIndex.JIBAR3M;
                Date dValueDate = ExcelUtilities.GetDates(valueDate);
                IRSwap swapObj = ObjectMap.Instance.GetObjectFromID<IRSwap>(swap);
                DatesAndRates curveObj = ObjectMap.Instance.GetObjectFromID<DatesAndRates>(curve);
                IDiscountingSource discountCurve = curveObj;

                // Calculate the first fixing off the curve to use at all past dates.
                double df1 = discountCurve.GetDF(dValueDate);
                Date laterDate = dValueDate.AddTenor(index.tenor);
                double df2 = discountCurve.GetDF(laterDate);
                double dt = (laterDate - dValueDate) / 365.0;
                double rate = (df1 / df2 - 1) / dt;

                //Set up the valuation engine.
                IFloatingRateSource forecastCurve = new ForecastCurveFromDiscount(discountCurve, index,
                    new FloatingRateFixingCurve1Rate(rate, FloatingIndex.JIBAR3M));
                DeterminsiticCurves curveSim = new DeterminsiticCurves(Currency.ZAR, discountCurve);
                curveSim.AddForecast(forecastCurve);
                Coordinator coordinator = new Coordinator(curveSim, new List<Product> { swapObj }, new List<Simulator>(), 1);

                // Run the valuation
                double value = coordinator.Value(dValueDate);
                return value;
            }
            catch (Exception e)
            {
                return ExcelUtilities.Error0D(e);
            }
        }
    }
}
