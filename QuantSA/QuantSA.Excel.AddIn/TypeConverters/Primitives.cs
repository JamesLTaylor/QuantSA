using System;
using QuantSA.Excel.Shared;
using QuantSA.ExcelFunctions;
using QuantSA.General;
using QuantSA.General.Conventions.BusinessDay;
using QuantSA.General.Conventions.DayCount;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.Excel.Addin.TypeConverters
{
    /// <summary>
    /// Converts from an Excel date or a string with value "TODAY".
    /// </summary>
    public class DateConverter : IInputConverter
    {
        public Type RequiredType => typeof(Date);

        public object Convert(object input, string inputName, string defaultValue)
        {
            if (input == null && defaultValue == null)
                return null;
            var strValue = input == null ? defaultValue : input as string;
            if (strValue != null)
                if (strValue.ToUpper() == "TODAY")
                    return new Date(DateTime.Today);
            if (!(input is double value))
                throw new ArgumentException($"{inputName} must have a value representing an Excel Date.");
            return new Date(DateTime.FromOADate(value));
        }
    }

    public class DateOutputConverter : IOutputConverter
    {
        public Type SuppliedType => typeof(Date);

        public object Convert(object input)
        {
            return ((Date) input).ToOADate();
        }
    }

    public class CurrencyConverter : IInputConverter
    {
        public Type RequiredType => typeof(Currency);

        public object Convert(object input, string inputName, string defaultValue)
        {
            var strValue = input == null ? defaultValue : input as string;
            if (strValue is null)
                throw new ArgumentException($"{inputName} must have a string value representing a Currency.");
            return new Currency(strValue);
        }
    }

    public class CalendarConverter : IInputConverter
    {
        public Type RequiredType => typeof(Calendar);

        public object Convert(object input, string inputName, string defaultValue)
        {
            var strValue = input == null ? defaultValue : input as string;
            if (strValue is null) throw new ArgumentException($"{inputName} must be the name of a saved calendar.");
            return StaticData.GetCalendar(strValue.ToUpper());
        }
    }

    public class BusinessDayConventionConverter : IInputConverter
    {
        public Type RequiredType => typeof(BusinessDayConvention);

        public object Convert(object input, string inputName, string defaultValue)
        {
            var strValue = input == null ? defaultValue : input as string;
            if (strValue is null)
                throw new ArgumentException(
                    $"{inputName} must be one of the strings representing a BusinessDayConvention.");
            switch (strValue.ToUpper())
            {
                case "F":
                case "FOLLOWING": return BusinessDayStore.Following;
                case "MF":
                case "MODFOLLOW":
                case "MODIFIEDFOLLOWING": return BusinessDayStore.ModifiedFollowing;
                case "P":
                case "PRECEDING": return BusinessDayStore.Preceding;
                case "MP":
                case "MODIFIEDPRECEDING": return BusinessDayStore.ModifiedPreceding;
                case "U":
                case "UNADJUSTED": return BusinessDayStore.Unadjusted;

                default:
                    throw new ArgumentException(
                        strValue + " is not a known business day convention in input: " + inputName);
            }
        }
    }

    public class DayCountConventionConverter : IInputConverter
    {
        public Type RequiredType => typeof(DayCountConvention);

        public object Convert(object input, string inputName, string defaultValue)
        {
            var strValue = input == null ? defaultValue : input as string;
            if (strValue is null)
                throw new ArgumentException(
                    $"{inputName} must be one of the strings representing a DayCountConvention.");
            switch (strValue.ToUpper())
            {
                case "ACTACT": return DayCountStore.ActActISDA;
                case "ACT360": return DayCountStore.Actual360;
                case "ACT365F":
                case "ACT365": return DayCountStore.Actual365Fixed;
                case "30360EU": return DayCountStore.Thirty360Euro;

                default: throw new ArgumentException(strValue + " is not a known day count convention: " + inputName);
            }
        }
    }

    public class FloatingIndexConverter : IInputConverter
    {
        public Type RequiredType => typeof(FloatRateIndex);

        public object Convert(object input, string inputName, string defaultValue)
        {
            var strValue = input == null ? defaultValue : input as string;
            if (strValue is null)
                throw new ArgumentException($"{inputName} must be one of the strings representing a FloatRateIndex.");
            switch (strValue.ToUpper())
            {
                case "JIBAR1M": return FloatRateIndex.JIBAR1M;
                case "JIBAR3M": return FloatRateIndex.JIBAR3M;
                case "JIBAR6M": return FloatRateIndex.JIBAR6M;
                case "PRIME1M_AVG": return FloatRateIndex.PRIME1M_AVG;
                case "LIBOR1M": return FloatRateIndex.LIBOR1M;
                case "LIBOR3M": return FloatRateIndex.LIBOR3M;
                case "LIBOR6M": return FloatRateIndex.LIBOR6M;
                case "EURIBOR3M": return FloatRateIndex.EURIBOR3M;
                case "EURIBOR6M": return FloatRateIndex.EURIBOR6M;
                default:
                    throw new ArgumentException(strValue + " is not a known floating rate index in input: " +
                                                inputName);
            }
        }
    }

    public class TenorConverter : IInputConverter
    {
        public Type RequiredType => typeof(Tenor);

        public object Convert(object input, string inputName, string defaultValue)
        {
            var strValue = input == null ? defaultValue : input as string;
            if (strValue is null) throw new ArgumentException($"{inputName} must be a string representing a Tenor.");
            var numberStr = "";
            var years = 0;
            var months = 0;
            var weeks = 0;
            var days = 0;
            foreach (var c in strValue.ToUpper())
                if (c >= 48 && c <= 57)
                {
                    numberStr += c;
                }
                else if (c == 'Y')
                {
                    years = int.Parse(numberStr);
                    numberStr = "";
                }
                else if (c == 'M')
                {
                    months = int.Parse(numberStr);
                    numberStr = "";
                }
                else if (c == 'W')
                {
                    weeks = int.Parse(numberStr);
                    numberStr = "";
                }
                else if (c == 'D')
                {
                    days = int.Parse(numberStr);
                    numberStr = "";
                }
                else
                {
                    throw new ArgumentException(strValue + " is not a valid tenor String.");
                }

            return new Tenor(days, weeks, months, years);
        }
    }

    public class ReferenceEntityConverter : IInputConverter
    {
        public Type RequiredType => typeof(ReferenceEntity);

        public object Convert(object input, string inputName, string defaultValue)
        {
            var strValue = input == null ? defaultValue : input as string;
            if (strValue is null)
                throw new ArgumentException($"{inputName} must be a string representing a ReferenceEntity.");
            return new ReferenceEntity(strValue);
        }
    }

    /// <summary>
    /// Return a share from a string of form 'ZAR.ALSI'
    /// </summary>
    public class ShareConverter : IInputConverter
    {
        public Type RequiredType => typeof(Share);

        public object Convert(object input, string inputName, string defaultValue)
        {
            var strValue = input == null ? defaultValue : input as string;
            if (strValue is null) throw new ArgumentException($"{inputName} must be a string representing a Share.");
            var parts = strValue.Split('.');
            if (parts.Length != 2)
                throw new ArgumentException(strValue + " in " + inputName + " does not correspond to a share.");
            var ccy = new CurrencyConverter().Convert(parts[0], inputName, null) as Currency;
            return new Share(parts[1].ToUpper(), ccy);
        }
    }
}