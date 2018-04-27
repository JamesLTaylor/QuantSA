using QuantSA.General;
using QuantSA.Excel.Common;
using QuantSA.General.Conventions.Compounding;
using System;
using QuantSA.General.Conventions.BusinessDay;
using QuantSA.Primitives.Dates;
using QuantSA.General.Conventions.DayCount;
using QuantSA.General.Dates;
using QuantSA.Primitives.Dates;

namespace QuantSA.ExcelFunctions
{
    public class XLConventions
    {
        [QuantSAExcelFunction(Description = "Convert an interest rate from one compoundin convention to another.",
            Name = "QSA.RateConvert",
            HasGeneratedVersion = true,
            Category = "QSA.Conventions",
            ExampleSheet = "Conventions.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/RateConvert.html")]
        public static double RateConvert([QuantSAExcelArgument(Description = "The rate to convert.")]double rate,
            [QuantSAExcelArgument(Description = "The compounding convention of the input rate.")]CompoundingConvention compoundingFrom,
            [QuantSAExcelArgument(Description = "The compounding convention that the output rate should be in.")]CompoundingConvention compoundingTo,
            [QuantSAExcelArgument(Description = "(Optional) The yearfraction over which the rate applies.  Only required if one of the conventions is 'Simple' or 'Discount'", Default = "double.NaN")]double yearFraction)

        {
            if (compoundingFrom == CompoundingStore.Simple && double.IsNaN(yearFraction))
                throw new ArgumentException("Cannot convert from a 'Simple' convention without the year fraction being specified.");
            if (compoundingFrom == CompoundingStore.Discount && double.IsNaN(yearFraction))
                throw new ArgumentException("Cannot convert from a 'Discount' convention without the year fraction being specified.");
            if (compoundingTo == CompoundingStore.Simple && double.IsNaN(yearFraction))
                throw new ArgumentException("Cannot convert from a 'Simple' convention without the year fraction being specified.");
            if (compoundingTo == CompoundingStore.Discount && double.IsNaN(yearFraction))
                throw new ArgumentException("Cannot convert to a 'Discount' convention without the year fraction being specified.");

            if (double.IsNaN(yearFraction)) yearFraction = 1.0;
            double df = compoundingFrom.DF(rate, yearFraction);
            double resultRate = compoundingTo.rateFromDF(df, yearFraction);
            return resultRate;
        }

        [QuantSAExcelFunction(Description = "Get the discont factor implied by an interest rate of the given convention.",
            Name = "QSA.DFFromRate",
            HasGeneratedVersion = true,
            Category = "QSA.Conventions",
            ExampleSheet = "Conventions.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/DFFromRate.html")]
        public static double DFFromRate([QuantSAExcelArgument(Description = "The rate to use in finding the discount factor.")]double rate,
            [QuantSAExcelArgument(Description = "The compounding convention of the input rate.")]CompoundingConvention compounding,            
            [QuantSAExcelArgument(Description = "The year fraction over which the rate applies.")]double yearFraction)

        {
            return compounding.DF(rate, yearFraction);
        }

        [QuantSAExcelFunction(Description = "Adjust the provided date according to the given business day convention and calendar.",
            Name = "QSA.ApplyBusinessDayAdjustment",
            HasGeneratedVersion = true,
            Category = "QSA.Conventions",
            ExampleSheet = "Conventions.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/ApplyBusinessDayAdjustment.html")]
        public static Date ApplyBusinessDayAdjustment([QuantSAExcelArgument(Description = "The date to be adjusted")]Date date,
            [QuantSAExcelArgument(Description = "The business day rule to apply to the date.")]BusinessDayConvention convention,
            [QuantSAExcelArgument(Description = "The calendar to use in the adjustment.")]Calendar calendar)

        {
            return convention.Adjust(date, calendar);
        }

        [QuantSAExcelFunction(Description = "Check if the provided date is a holiday in the selected calendar.  Does not check for weekends.",
            Name = "QSA.IsHoliday",
            HasGeneratedVersion = true,
            Category = "QSA.Conventions",
            ExampleSheet = "Conventions.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/IsHoliday.html")]
        public static double IsHoliday([QuantSAExcelArgument(Description = "The date to check")]Date date,    
            [QuantSAExcelArgument(Description = "The calendar to use.")]Calendar calendar)

        {
            return calendar.isHoliday(date) ? 1 : 0;
        }

        [QuantSAExcelFunction(Description = "Get the year fraction or accrual fraction between two dates according to the provided convention.",
            Name = "QSA.GetYearFraction",
            HasGeneratedVersion = true,
            Category = "QSA.Conventions",
            ExampleSheet = "Conventions.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetYearFraction.html")]
        public static double GetYearFraction([QuantSAExcelArgument(Description = "The first date.")]Date date1,
            [QuantSAExcelArgument(Description = "The second date.")]Date date2,
            [QuantSAExcelArgument(Description = "The day count convention to use for getting the accrual fraction.")]DayCountConvention convention)

        {
            return convention.YearFraction(date1, date2);            
        }

        [QuantSAExcelFunction(Description = "Create an actual 252 daycount convention, this is can't be done directly from a string like the other conventions because it needs a calendar.",
            Name = "QSA.CreateBusiness252DayCount",
            HasGeneratedVersion = true,
            Category = "QSA.Conventions",
            ExampleSheet = "Conventions.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateBusiness252DayCount.html")]
        public static DayCountConvention CreateBusiness252DayCount([QuantSAExcelArgument(Description = "The calendar to use.")]Calendar calendar)
        {
            return DayCountStore.Business252(calendar);
        }
    }
}