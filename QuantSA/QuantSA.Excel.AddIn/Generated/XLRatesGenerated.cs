using System;
using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.General;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLRatesGenerated
    {
        [QuantSAExcelFunction(Name = "QSA.CreateFixedLeg", IsGeneratedVersion = true)]
        public static object _CreateFixedLeg(string objectName,
            object[,] currency,
            object[,] paymentDates,
            object[,] notionals,
            object[,] rates,
            object[,] accrualFractions)
        {
            try
            {
                var _currency = XU.GetSpecialType0D<Currency>(currency, "currency");
                var _paymentDates = XU.GetDate1D(paymentDates, "paymentDates");
                var _notionals = XU.GetDouble1D(notionals, "notionals");
                var _rates = XU.GetDouble1D(rates, "rates");
                var _accrualFractions = XU.GetDouble1D(accrualFractions, "accrualFractions");
                var _result = XLRates.CreateFixedLeg(_currency, _paymentDates, _notionals, _rates, _accrualFractions);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateFloatLeg", IsGeneratedVersion = true)]
        public static object _CreateFloatLeg(string objectName,
            object[,] currency,
            object[,] floatingIndex,
            object[,] resetDates,
            object[,] paymentDates,
            object[,] notionals,
            object[,] spreads,
            object[,] accrualFractions)
        {
            try
            {
                var _currency = XU.GetSpecialType0D<Currency>(currency, "currency");
                var _floatingIndex = XU.GetSpecialType0D<FloatingIndex>(floatingIndex, "floatingIndex");
                var _resetDates = XU.GetDate1D(resetDates, "resetDates");
                var _paymentDates = XU.GetDate1D(paymentDates, "paymentDates");
                var _notionals = XU.GetDouble1D(notionals, "notionals");
                var _spreads = XU.GetDouble1D(spreads, "spreads");
                var _accrualFractions = XU.GetDouble1D(accrualFractions, "accrualFractions");
                var _result = XLRates.CreateFloatLeg(_currency, _floatingIndex, _resetDates, _paymentDates, _notionals,
                    _spreads, _accrualFractions);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateCashLeg", IsGeneratedVersion = true)]
        public static object _CreateCashLeg(string objectName,
            object[,] paymentDates,
            object[,] amounts,
            object[,] currencies)
        {
            try
            {
                var _paymentDates = XU.GetDate1D(paymentDates, "paymentDates");
                var _amounts = XU.GetDouble1D(amounts, "amounts");
                var _currencies = XU.GetSpecialType1D<Currency>(currencies, "currencies");
                var _result = XLRates.CreateCashLeg(_paymentDates, _amounts, _currencies);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateZARBermudanSwaption", IsGeneratedVersion = true)]
        public static object _CreateZARBermudanSwaption(string objectName,
            object[,] exerciseDates,
            object[,] longOptionality,
            object[,] startDate,
            object[,] tenor,
            object[,] rate,
            object[,] payFixed,
            object[,] notional)
        {
            try
            {
                var _exerciseDates = XU.GetDate1D(exerciseDates, "exerciseDates");
                var _longOptionality = XU.GetBoolean0D(longOptionality, "longOptionality");
                var _startDate = XU.GetDate0D(startDate, "startDate");
                var _tenor = XU.GetSpecialType0D<Tenor>(tenor, "tenor");
                var _rate = XU.GetDouble0D(rate, "rate");
                var _payFixed = XU.GetBoolean0D(payFixed, "payFixed");
                var _notional = XU.GetDouble0D(notional, "notional");
                var _result = XLRates.CreateZARBermudanSwaption(_exerciseDates, _longOptionality, _startDate, _tenor,
                    _rate, _payFixed, _notional);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


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
                var _startDate = XU.GetDate0D(startDate, "startDate");
                var _tenor = XU.GetSpecialType0D<Tenor>(tenor, "tenor");
                var _rate = XU.GetDouble0D(rate, "rate");
                var _payFixed = XU.GetBoolean0D(payFixed, "payFixed");
                var _notional = XU.GetDouble0D(notional, "notional");
                var _result = XLRates.CreateZARSwap(_startDate, _tenor, _rate, _payFixed, _notional);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateZARFRA", IsGeneratedVersion = true)]
        public static object _CreateZARFRA(string objectName,
            object[,] tradeDate,
            object[,] notional,
            object[,] rate,
            object[,] fraCode,
            object[,] payFixed)
        {
            try
            {
                var _tradeDate = XU.GetDate0D(tradeDate, "tradeDate");
                var _notional = XU.GetDouble0D(notional, "notional");
                var _rate = XU.GetDouble0D(rate, "rate");
                var _fraCode = XU.GetSpecialType0D<string>(fraCode, "fraCode");
                var _payFixed = XU.GetBoolean0D(payFixed, "payFixed");
                var _result = XLRates.CreateZARFRA(_tradeDate, _notional, _rate, _fraCode, _payFixed);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.ValueZARSwap", IsGeneratedVersion = true)]
        public static object _ValueZARSwap1Curve(object[,] swap,
            object[,] valueDate,
            object[,] curve)
        {
            try
            {
                var _swap = XU.GetObject0D<IRSwap>(swap, "swap");
                var _valueDate = XU.GetDate0D(valueDate, "valueDate");
                var _curve = XU.GetObject0D<IDiscountingSource>(curve, "curve");
                var _result = XLRates.ValueZARSwap1Curve(_swap, _valueDate, _curve);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateRateForecastCurveFromDiscount", IsGeneratedVersion = true)]
        public static object _CreateRateForecastCurveFromDiscount(string objectName,
            object[,] floatingRateIndex,
            object[,] discountCurve,
            object[,] fixingCurve)
        {
            try
            {
                var _floatingRateIndex = XU.GetSpecialType0D<FloatingIndex>(floatingRateIndex, "floatingRateIndex");
                var _discountCurve = XU.GetObject0D<IDiscountingSource>(discountCurve, "discountCurve");
                var _fixingCurve = XU.GetObject0D<IFloatingRateSource>(fixingCurve, "fixingCurve", null);
                var _result =
                    XLRates.CreateRateForecastCurveFromDiscount(_floatingRateIndex, _discountCurve, _fixingCurve);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.GetDF", IsGeneratedVersion = true)]
        public static object _GetDF(object[,] curve,
            object[,] date)
        {
            try
            {
                var _curve = XU.GetObject0D<IDiscountingSource>(curve, "curve");
                var _date = XU.GetDate0D(date, "date");
                var _result = XLRates.GetDF(_curve, _date);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateLoanFixedRate", IsGeneratedVersion = true)]
        public static object _CreateLoanFixedRate(string objectName,
            object[,] currency,
            object[,] balanceDates,
            object[,] balanceAmounts,
            object[,] fixedRate)
        {
            try
            {
                var _currency = XU.GetSpecialType0D<Currency>(currency, "currency");
                var _balanceDates = XU.GetDate1D(balanceDates, "balanceDates");
                var _balanceAmounts = XU.GetDouble1D(balanceAmounts, "balanceAmounts");
                var _fixedRate = XU.GetDouble0D(fixedRate, "fixedRate");
                var _result = XLRates.CreateLoanFixedRate(_currency, _balanceDates, _balanceAmounts, _fixedRate);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateLoanFloatingRate", IsGeneratedVersion = true)]
        public static object _CreateLoanFloatingRate(string objectName,
            object[,] currency,
            object[,] balanceDates,
            object[,] balanceAmounts,
            object[,] floatingIndex,
            object[,] floatingSpread)
        {
            try
            {
                var _currency = XU.GetSpecialType0D<Currency>(currency, "currency");
                var _balanceDates = XU.GetDate1D(balanceDates, "balanceDates");
                var _balanceAmounts = XU.GetDouble1D(balanceAmounts, "balanceAmounts");
                var _floatingIndex = XU.GetSpecialType0D<FloatingIndex>(floatingIndex, "floatingIndex");
                var _floatingSpread = XU.GetDouble0D(floatingSpread, "floatingSpread");
                var _result = XLRates.CreateLoanFloatingRate(_currency, _balanceDates, _balanceAmounts, _floatingIndex,
                    _floatingSpread);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateHWModelDemo", IsGeneratedVersion = true)]
        public static object _CreateHWModelDemo(string objectName,
            object[,] meanReversion,
            object[,] flatVol,
            object[,] baseCurve,
            object[,] forecastIndices)
        {
            try
            {
                var _meanReversion = XU.GetDouble0D(meanReversion, "meanReversion");
                var _flatVol = XU.GetDouble0D(flatVol, "flatVol");
                var _baseCurve = XU.GetObject0D<IDiscountingSource>(baseCurve, "baseCurve");
                var _forecastIndices = XU.GetSpecialType0D<FloatingIndex>(forecastIndices, "forecastIndices");
                var _result = XLRates.CreateHWModelDemo(_meanReversion, _flatVol, _baseCurve, _forecastIndices);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }
    }
}