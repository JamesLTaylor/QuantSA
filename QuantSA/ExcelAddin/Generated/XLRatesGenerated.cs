using System;
using XU = QuantSA.Excel.ExcelUtilities;
using QuantSA.General;
using QuantSA.Valuation;
using QuantSA.ExcelFunctions;
using QuantSA.Excel.Common;
using QuantSA.General.Products.Rates;

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
                Currency _currency = XU.GetCurrency0D(currency, "currency");
                Date[] _paymentDates = XU.GetDate1D(paymentDates, "paymentDates");
                Double[] _notionals = XU.GetDouble1D(notionals, "notionals");
                Double[] _rates = XU.GetDouble1D(rates, "rates");
                Double[] _accrualFractions = XU.GetDouble1D(accrualFractions, "accrualFractions");
                FixedLeg _result = XLRates.CreateFixedLeg(_currency, _paymentDates, _notionals, _rates, _accrualFractions);
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
                Currency _currency = XU.GetCurrency0D(currency, "currency");
                FloatingIndex _floatingIndex = XU.GetFloatingIndex0D(floatingIndex, "floatingIndex");
                Date[] _resetDates = XU.GetDate1D(resetDates, "resetDates");
                Date[] _paymentDates = XU.GetDate1D(paymentDates, "paymentDates");
                Double[] _notionals = XU.GetDouble1D(notionals, "notionals");
                Double[] _spreads = XU.GetDouble1D(spreads, "spreads");
                Double[] _accrualFractions = XU.GetDouble1D(accrualFractions, "accrualFractions");
                FloatLeg _result = XLRates.CreateFloatLeg(_currency, _floatingIndex, _resetDates, _paymentDates, _notionals, _spreads, _accrualFractions);
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
                Date[] _paymentDates = XU.GetDate1D(paymentDates, "paymentDates");
                Double[] _amounts = XU.GetDouble1D(amounts, "amounts");
                Currency[] _currencies = XU.GetCurrency1D(currencies, "currencies");
                CashLeg _result = XLRates.CreateCashLeg(_paymentDates, _amounts, _currencies);
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
                Date[] _exerciseDates = XU.GetDate1D(exerciseDates, "exerciseDates");
                Boolean _longOptionality = XU.GetBoolean0D(longOptionality, "longOptionality");
                Date _startDate = XU.GetDate0D(startDate, "startDate");
                Tenor _tenor = XU.GetTenor0D(tenor, "tenor");
                Double _rate = XU.GetDouble0D(rate, "rate");
                Boolean _payFixed = XU.GetBoolean0D(payFixed, "payFixed");
                Double _notional = XU.GetDouble0D(notional, "notional");
                BermudanSwaption _result = XLRates.CreateZARBermudanSwaption(_exerciseDates, _longOptionality, _startDate, _tenor, _rate, _payFixed, _notional);
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
                Date _startDate = XU.GetDate0D(startDate, "startDate");
                Tenor _tenor = XU.GetTenor0D(tenor, "tenor");
                Double _rate = XU.GetDouble0D(rate, "rate");
                Boolean _payFixed = XU.GetBoolean0D(payFixed, "payFixed");
                Double _notional = XU.GetDouble0D(notional, "notional");
                IRSwap _result = XLRates.CreateZARSwap(_startDate, _tenor, _rate, _payFixed, _notional);
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
                Date _tradeDate = XU.GetDate0D(tradeDate, "tradeDate");
                Double _notional = XU.GetDouble0D(notional, "notional");
                Double _rate = XU.GetDouble0D(rate, "rate");
                String _fraCode = XU.GetString0D(fraCode, "fraCode");
                Boolean _payFixed = XU.GetBoolean0D(payFixed, "payFixed");
                FRA _result = XLRates.CreateZARFRA(_tradeDate, _notional, _rate, _fraCode, _payFixed);
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
                IRSwap _swap = XU.GetObject0D<IRSwap>(swap, "swap");
                Date _valueDate = XU.GetDate0D(valueDate, "valueDate");
                IDiscountingSource _curve = XU.GetObject0D<IDiscountingSource>(curve, "curve");
                Double _result = XLRates.ValueZARSwap1Curve(_swap, _valueDate, _curve);
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
                FloatingIndex _floatingRateIndex = XU.GetFloatingIndex0D(floatingRateIndex, "floatingRateIndex");
                IDiscountingSource _discountCurve = XU.GetObject0D<IDiscountingSource>(discountCurve, "discountCurve");
                IFloatingRateSource _fixingCurve = XU.GetObject0D<IFloatingRateSource>(fixingCurve, "fixingCurve", null);
                IFloatingRateSource _result = XLRates.CreateRateForecastCurveFromDiscount(_floatingRateIndex, _discountCurve, _fixingCurve);
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
                IDiscountingSource _curve = XU.GetObject0D<IDiscountingSource>(curve, "curve");
                Date _date = XU.GetDate0D(date, "date");
                Double _result = XLRates.GetDF(_curve, _date);
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
                Currency _currency = XU.GetCurrency0D(currency, "currency");
                Date[] _balanceDates = XU.GetDate1D(balanceDates, "balanceDates");
                Double[] _balanceAmounts = XU.GetDouble1D(balanceAmounts, "balanceAmounts");
                Double _fixedRate = XU.GetDouble0D(fixedRate, "fixedRate");
                LoanFixedRate _result = XLRates.CreateLoanFixedRate(_currency, _balanceDates, _balanceAmounts, _fixedRate);
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
                Currency _currency = XU.GetCurrency0D(currency, "currency");
                Date[] _balanceDates = XU.GetDate1D(balanceDates, "balanceDates");
                Double[] _balanceAmounts = XU.GetDouble1D(balanceAmounts, "balanceAmounts");
                FloatingIndex _floatingIndex = XU.GetFloatingIndex0D(floatingIndex, "floatingIndex");
                Double _floatingSpread = XU.GetDouble0D(floatingSpread, "floatingSpread");
                LoanFloatingRate _result = XLRates.CreateLoanFloatingRate(_currency, _balanceDates, _balanceAmounts, _floatingIndex, _floatingSpread);
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
                Double _meanReversion = XU.GetDouble0D(meanReversion, "meanReversion");
                Double _flatVol = XU.GetDouble0D(flatVol, "flatVol");
                IDiscountingSource _baseCurve = XU.GetObject0D<IDiscountingSource>(baseCurve, "baseCurve");
                FloatingIndex _forecastIndices = XU.GetFloatingIndex0D(forecastIndices, "forecastIndices");
                HullWhite1F _result = XLRates.CreateHWModelDemo(_meanReversion, _flatVol, _baseCurve, _forecastIndices);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

    }
}
