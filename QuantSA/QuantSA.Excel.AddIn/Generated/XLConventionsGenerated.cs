using System;
using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.General.Conventions.BusinessDay;
using QuantSA.General.Conventions.Compounding;
using QuantSA.General.Conventions.DayCount;
using QuantSA.General.Dates;
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
                var _rate = XU.GetDouble0D(rate, "rate");
                var _compoundingFrom = XU.GetSpecialType0D<CompoundingConvention>(compoundingFrom, "compoundingFrom");
                var _compoundingTo = XU.GetSpecialType0D<CompoundingConvention>(compoundingTo, "compoundingTo");
                var _yearFraction = XU.GetDouble0D(yearFraction, "yearFraction", double.NaN);
                var _result = XLConventions.RateConvert(_rate, _compoundingFrom, _compoundingTo, _yearFraction);
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
                var _rate = XU.GetDouble0D(rate, "rate");
                var _compounding = XU.GetSpecialType0D<CompoundingConvention>(compounding, "compounding");
                var _yearFraction = XU.GetDouble0D(yearFraction, "yearFraction");
                var _result = XLConventions.DFFromRate(_rate, _compounding, _yearFraction);
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
                var _date = XU.GetDate0D(date, "date");
                var _convention = XU.GetSpecialType0D<BusinessDayConvention>(convention, "convention");
                var _calendar = XU.GetSpecialType0D<Calendar>(calendar, "calendar");
                var _result = XLConventions.ApplyBusinessDayAdjustment(_date, _convention, _calendar);
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
                var _date = XU.GetDate0D(date, "date");
                var _calendar = XU.GetSpecialType0D<Calendar>(calendar, "calendar");
                var _result = XLConventions.IsHoliday(_date, _calendar);
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
                var _date1 = XU.GetDate0D(date1, "date1");
                var _date2 = XU.GetDate0D(date2, "date2");
                var _convention = XU.GetSpecialType0D<DayCountConvention>(convention, "convention");
                var _result = XLConventions.GetYearFraction(_date1, _date2, _convention);
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
                var _calendar = XU.GetSpecialType0D<Calendar>(calendar, "calendar");
                var _result = XLConventions.CreateBusiness252DayCount(_calendar);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }
    }
}