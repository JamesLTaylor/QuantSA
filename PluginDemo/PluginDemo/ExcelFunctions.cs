using QuantSA.Excel.Shared;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.Serialization;
using QuantSA.Shared.State;

namespace PluginDemo
{
    public class ExcelFunctions
    {
        /*
        [QuantSAExcelFunction(Description = "",
        Name = "QSDEMO.ShowAbout",
        Category = "QSDEMO",
            IsMacroType = true,
        IsHidden = true)]
        public static object ShowAbout()
        {
            ExcelMessage message = new ExcelMessage("QuantSA Plugin demo.", PluginConnection.instance.GetAboutString());
            message.ShowDialog();
            return null;
        }*/

        [QuantSAExcelFunction(Description = "",
            Name = "QSDEMO.CreateDiscount",
            Category = "QSDEMO",
            IsHidden = false)]
        public static IDiscountingSource CreateDiscount([QuantSAExcelArgument(Description = "")]
            Date anchorDate,
            [QuantSAExcelArgument(Description = "")]
            double rate)
        {
            var currency = QuantSAState.SharedData.Get<Currency>("ZAR");
            return new PluginDiscount(anchorDate, rate, currency);
        }

        [QuantSAExcelFunction(Description = "",
            Name = "QSDEMO.GetSpecialDF",
            Category = "QSDEMO",
            IsHidden = false)]
        public static object GetSpecialDF([QuantSAExcelArgument(Description = "The discounting curve.")]
            IDiscountingSource discountingSource,
            [QuantSAExcelArgument(Description = "date")]
            Date date)
        {
            return discountingSource.GetDF(date);
        }
    }
}