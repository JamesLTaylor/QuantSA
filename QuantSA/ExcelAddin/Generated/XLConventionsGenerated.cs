using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.General.Conventions.Compounding;
using System;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLConventionsGenerated
    {

        [QuantSAExcelFunction(Name = "QSA.RateConvert", IsGeneratedVersion = true)]
        public static object _RateConvert(object[,] rate,
                            object[,] compoundingFrom,
                            object[,] compoundingTo,
                            object[,] yearFraction)
        {
            try
            {
                Double _rate = XU.GetDouble0D(rate, "rate");
                CompoundingConvention _compoundingFrom = XU.GetSpecialType0D<CompoundingConvention>(compoundingFrom, "compoundingFrom");
                CompoundingConvention _compoundingTo = XU.GetSpecialType0D<CompoundingConvention>(compoundingTo, "compoundingTo");
                Double _yearFraction = XU.GetDouble0D(yearFraction, "yearFraction", double.NaN);
                Double _result = XLConventions.RateConvert(_rate, _compoundingFrom, _compoundingTo, _yearFraction);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

    }
}
