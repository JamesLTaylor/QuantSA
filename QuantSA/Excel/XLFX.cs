using ExcelDna.Integration;
using QuantSA.Excel;
using QuantSA.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLFX
    {
        [QuantSAExcelFunction(Description = "Create a curve to be used for FX rate forecasting.",
        Name = "QSA.CreateFXForecastCurve",
        Category = "QSA.FX",
        IsHidden = false,
        HelpTopic = "")]
        public static object CreateFXForecastCurve([ExcelArgument(Description = "Name of object")]String name,
            [ExcelArgument(Description = "")]object[,] baseCurrency,
            [ExcelArgument(Description = "")]object[,] counterCurrency,
            [ExcelArgument(Description = "")]object[,] fxRateAtAnchorDate,
            [ExcelArgument(Description = "")]object[,] baseCurrencyFXBasisCurve,
            [ExcelArgument(Description = "")]object[,] counterCurrencyFXBasisCurve)
        {
            try
            {
                FXForecastCurve fxForecastCurve = new FXForecastCurve(XU.GetCurrency0D(baseCurrency, "baseCurrency"),
                    XU.GetCurrency0D(counterCurrency, "counterCurrency"), XU.GetDoubles0D(fxRateAtAnchorDate, "fxRateAtAnchorDate"),
                    XU.GetObjects0D<IDiscountingSource>(baseCurrencyFXBasisCurve, "baseCurrencyFXBasisCurve"),
                    XU.GetObjects0D<IDiscountingSource>(counterCurrencyFXBasisCurve, "counterCurrencyFXBasisCurve"));
                return XU.AddObject(name, fxForecastCurve);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

        [QuantSAExcelFunction(Description = "Get the FX rate at a date.  There is no spot settlement adjustment.",
        Name = "QSA.GetFXRate",
        Category = "QSA.FX",
        IsHidden = false,
        HelpTopic = "")]
        public static object GetFXRate([ExcelArgument(Description = "Name of FX curve")]object[,] FXCurveName,
            [ExcelArgument(Description = "Date on which FX rate is required.")]object[,] date)
        {
            try
            {
                IFXSource fxCurve = XU.GetObjects0D<IFXSource>(FXCurveName, "FXCurveName");
                return fxCurve.GetRate(XU.GetDates0D(date, "date"));
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }



        [QuantSAExcelFunction(Description = "Create a cross currency interest rate swap with explicit dates.",
        Name = "QSA.CreateCCIRSWithDates",
        Category = "QSA.FX",
        IsHidden = false,
        HelpTopic = "")]
                public static object CreateCCIRSWithDates([ExcelArgument(Description = "Name of object")]String name,
        [ExcelArgument(Description = "")]object[] floatResetDates,
        [ExcelArgument(Description = "")]object[] paymentDates,
        [ExcelArgument(Description = "")]object[] notionals,
        [ExcelArgument(Description = "")]object payCurrency,
        [ExcelArgument(Description = "")]object recCurrency)
        {
            try
            {
                return "not implemented";
                //IRSwap swap = IRSwap.CreateZARSwap(rate, ExcelUtilities.GetBool(payFixed), notional,
                //    ExcelUtilities.GetDates(startDate), ExcelUtilities.GetTenor(tenor));
                //return ObjectMap.Instance.AddObject(name, swap);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }
    }
}
