using System;
using XU = QuantSA.Excel.ExcelUtilities;
using QuantSA.General;
using QuantSA.Valuation;

namespace QuantSA.Excel
{
    public class XLTestGenerated
    {
        [QuantSAExcelFunction(Name = "QSA.CreateDatesAndRatesCurveTest", IsGeneratedVersion=true)]
        public static object _CreateDatesAndRatesCurveTest(string objectName,
                            object[,] dates,
                            object[,] rates,
                            object[,] currency)
        {
            try
            {
                Date[] _dates = XU.GetDate1D(dates, "dates");
                double[] _rates = XU.GetDouble1D(rates, "rates");
                Currency _currency = XU.GetCurrency0D(currency, "currency");
                IDiscountingSource result = XLTest.CreateDatesAndRatesCurveTest(_dates, _rates, _currency);
                return XU.AddObject(objectName, result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }
    }
}
