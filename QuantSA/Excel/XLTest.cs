using ExcelDna.Integration;
using QuantSA.General;
using System;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLTest
    {
        [QuantSAExcelFunction(Description = "Create a curve of dates and rates.",
        Name = "QSA.CreateDatesAndRatesCurveTest",
            HasGeneratedVersion=true,
        Category = "QSA.Curves",
        IsHidden = false,
        HelpTopic = "http://www.quantsa.org/CreateDatesAndRatesCurve.html")]
        public static IDiscountingSource CreateDatesAndRatesCurveTest([QuantSAExcelArgument(Description = "The dates at which the rates are defined.")]Date[] dates,
            [QuantSAExcelArgument(Description = "The continuously compounded rates at each of the provided dates.")]double[] rates,
            [QuantSAExcelArgument(Description = "The currency that this curve can be used for. Actually this is a really long input.", Optional = true)]Currency currency)
        {
            DatesAndRates curve = new DatesAndRates(currency, dates[0], dates, rates);
            return curve;
        }
    }
}





