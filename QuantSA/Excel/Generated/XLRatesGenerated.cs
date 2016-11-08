using System;
using XU = QuantSA.Excel.ExcelUtilities;
using QuantSA.General;
using QuantSA.Valuation;

namespace QuantSA.Excel
{
    public class XLRatesGenerated
    {

        [QuantSAExcelFunction(Name = "QSA.CreateZARSwap", IsGeneratedVersion = true)]
        public static object _CreateZARSwap(string objectName,
                            object[,] startDate,
                            object[,] tenor,
                            object[,] rate,
                            object[,] payFixed,
                            object[,] notional)
        {
            try
            {
                Date _startDate = XU.GetDate0D(startDate, "startDate");
                Tenor _tenor = XU.GetTenor0D(tenor, "tenor");
                Double _rate = XU.GetDouble0D(rate, "rate");
                Boolean _payFixed = XU.GetBoolean0D(payFixed, "payFixed");
                Double _notional = XU.GetDouble0D(notional, "notional");
                Object _result = XLRates.CreateZARSwap(_startDate, _tenor, _rate, _payFixed, _notional);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

    }
}
