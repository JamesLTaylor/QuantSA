using ExcelDna.Integration;
using QuantSA.General;
using QuantSA.Excel;

namespace QuantSA.ExcelFunctions
{
    public class XLFX
    {
        [QuantSAExcelFunction(Description = "Create a curve to be used for FX rate forecasting.",
            Name = "QSA.CreateFXForecastCurve",
            HasGeneratedVersion = true,
            Category = "QSA.FX",
            ExampleSheet = "GeneralSwap.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateFXForecastCurve.html")]
        public static object CreateFXForecastCurve([ExcelArgument(Description = "The base currency.  Values are measured in units of counter currency per one base currency.(Currency)")]Currency baseCurrency,
            [ExcelArgument(Description = "The counter currency.  Values are measured in units of counter currency per one base currency.(Currency)")]Currency counterCurrency,
            [ExcelArgument(Description = "The rate at the anchor date of the two curves.")]double fxRateAtAnchorDate,
            [ExcelArgument(Description = "A curve that will be used to obatin forward rates.")]IDiscountingSource baseCurrencyFXBasisCurve,
            [ExcelArgument(Description = "A curve that will be used to obtain forward rates.")]IDiscountingSource counterCurrencyFXBasisCurve)
        {
            return new FXForecastCurve(baseCurrency, counterCurrency, fxRateAtAnchorDate, baseCurrencyFXBasisCurve,
                counterCurrencyFXBasisCurve);
        }

        [QuantSAExcelFunction(Description = "Get the FX rate at a date.  There is no spot settlement adjustment.",
            Name = "QSA.GetFXRate",
            HasGeneratedVersion = true,
            ExampleSheet = "Introduction.xlsx",
            Category = "QSA.FX",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetFXRate.html")]
        public static double GetFXRate([ExcelArgument(Description = "Name of FX curve")]IFXSource fxCurve,
            [ExcelArgument(Description = "Date on which FX rate is required.")]Date date)
        {
            return fxCurve.GetRate(date);
        }
    }
}
