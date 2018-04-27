using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.General.Conventions.BusinessDay;
using QuantSA.General.Conventions.Compounding;
using QuantSA.General.Conventions.DayCount;
using QuantSA.Primitives.Dates;
using System;
using QuantSA.General.Dates;
using QuantSA.Primitives.Dates;
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


        [QuantSAExcelFunction(Name = "QSA.DFFromRate", IsGeneratedVersion = true)]
        public static object _DFFromRate(object[,] rate,
                            object[,] compounding,
                            object[,] yearFraction)
        {
            try
            {
                Double _rate = XU.GetDouble0D(rate, "rate");
                CompoundingConvention _compounding = XU.GetSpecialType0D<CompoundingConvention>(compounding, "compounding");
                Double _yearFraction = XU.GetDouble0D(yearFraction, "yearFraction");
                Double _result = XLConventions.DFFromRate(_rate, _compounding, _yearFraction);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.ApplyBusinessDayAdjustment", IsGeneratedVersion = true)]
        public static object _ApplyBusinessDayAdjustment(object[,] date,
                            object[,] convention,
                            object[,] calendar)
        {
            try
            {
                Date _date = XU.GetDate0D(date, "date");
                BusinessDayConvention _convention = XU.GetSpecialType0D<BusinessDayConvention>(convention, "convention");
                Calendar _calendar = XU.GetSpecialType0D<Calendar>(calendar, "calendar");
                Date _result = XLConventions.ApplyBusinessDayAdjustment(_date, _convention, _calendar);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.IsHoliday", IsGeneratedVersion = true)]
        public static object _IsHoliday(object[,] date,
                            object[,] calendar)
        {
            try
            {
                Date _date = XU.GetDate0D(date, "date");
                Calendar _calendar = XU.GetSpecialType0D<Calendar>(calendar, "calendar");
                Double _result = XLConventions.IsHoliday(_date, _calendar);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.GetYearFraction", IsGeneratedVersion = true)]
        public static object _GetYearFraction(object[,] date1,
                            object[,] date2,
                            object[,] convention)
        {
            try
            {
                Date _date1 = XU.GetDate0D(date1, "date1");
                Date _date2 = XU.GetDate0D(date2, "date2");
                DayCountConvention _convention = XU.GetSpecialType0D<DayCountConvention>(convention, "convention");
                Double _result = XLConventions.GetYearFraction(_date1, _date2, _convention);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateBusiness252DayCount", IsGeneratedVersion = true)]
        public static object _CreateBusiness252DayCount(string objectName,
                            object[,] calendar)
        {
            try
            {
                Calendar _calendar = XU.GetSpecialType0D<Calendar>(calendar, "calendar");
                DayCountConvention _result = XLConventions.CreateBusiness252DayCount(_calendar);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

    }
}
