using ExcelDna.Integration;
using QuantSA.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.Excel
{
    public class XLFX
    {
        [QuantSAExcelFunction(Description = "Create a cross currency interest rate swap with explicit date.",
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
                return e.Message;
            }
        }
    }
}
