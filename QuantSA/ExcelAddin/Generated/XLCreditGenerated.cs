using System;
using QuantSA.Excel.Common;
using QuantSA.ExcelFunctions;
using QuantSA.General;
using XU = QuantSA.Excel.ExcelUtilities;

namespace QuantSA.Excel
{
    public class XLCreditGenerated
    {
        [QuantSAExcelFunction(Name = "QSA.CreateCDS", IsGeneratedVersion = true)]
        public static object _CreateCDS(string objectName,
            object[,] refEntity,
            object[,] ccy,
            object[,] paymentDates,
            object[,] notionals,
            object[,] rates,
            object[,] accrualFractions,
            object[,] boughtProtection)
        {
            try
            {
                var _refEntity = XU.GetSpecialType0D<ReferenceEntity>(refEntity, "refEntity");
                var _ccy = XU.GetSpecialType0D<Currency>(ccy, "ccy");
                var _paymentDates = XU.GetDate1D(paymentDates, "paymentDates");
                var _notionals = XU.GetDouble1D(notionals, "notionals");
                var _rates = XU.GetDouble1D(rates, "rates");
                var _accrualFractions = XU.GetDouble1D(accrualFractions, "accrualFractions");
                var _boughtProtection = XU.GetBoolean0D(boughtProtection, "boughtProtection");
                var _result = XLCredit.CreateCDS(_refEntity, _ccy, _paymentDates, _notionals, _rates, _accrualFractions,
                    _boughtProtection);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateModelDeterministicCreditWithFXJump", IsGeneratedVersion = true)]
        public static object _CreateModelDeterministicCreditWithFXJump(string objectName,
            object[,] survivalProbSource,
            object[,] otherCurrency,
            object[,] fxSource,
            object[,] valueCurrencyDiscount,
            object[,] fxVol,
            object[,] relJumpSizeInDefault,
            object[,] expectedRecoveryRate)
        {
            try
            {
                var _survivalProbSource =
                    XU.GetObject0D<ISurvivalProbabilitySource>(survivalProbSource, "survivalProbSource");
                var _otherCurrency = XU.GetSpecialType0D<Currency>(otherCurrency, "otherCurrency");
                var _fxSource = XU.GetObject0D<IFXSource>(fxSource, "fxSource");
                var _valueCurrencyDiscount =
                    XU.GetObject0D<IDiscountingSource>(valueCurrencyDiscount, "valueCurrencyDiscount");
                var _fxVol = XU.GetDouble0D(fxVol, "fxVol");
                var _relJumpSizeInDefault = XU.GetDouble0D(relJumpSizeInDefault, "relJumpSizeInDefault");
                var _expectedRecoveryRate = XU.GetDouble0D(expectedRecoveryRate, "expectedRecoveryRate");
                var _result = XLCredit.CreateModelDeterministicCreditWithFXJump(_survivalProbSource, _otherCurrency,
                    _fxSource, _valueCurrencyDiscount, _fxVol, _relJumpSizeInDefault, _expectedRecoveryRate);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.CreateHazardCurve", IsGeneratedVersion = true)]
        public static object _CreateHazardCurve(string objectName,
            object[,] referenceEntity,
            object[,] anchorDate,
            object[,] dates,
            object[,] hazardRates)
        {
            try
            {
                var _referenceEntity = XU.GetSpecialType0D<ReferenceEntity>(referenceEntity, "referenceEntity");
                var _anchorDate = XU.GetDate0D(anchorDate, "anchorDate");
                var _dates = XU.GetDate1D(dates, "dates");
                var _hazardRates = XU.GetDouble1D(hazardRates, "hazardRates");
                var _result = XLCredit.CreateHazardCurve(_referenceEntity, _anchorDate, _dates, _hazardRates);
                return XU.AddObject(objectName, _result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }


        [QuantSAExcelFunction(Name = "QSA.GetSurvivalProb", IsGeneratedVersion = true)]
        public static object _GetSurvivalProb(object[,] survivalProbabilitySource,
            object[,] date1,
            object[,] date2)
        {
            try
            {
                var _survivalProbabilitySource =
                    XU.GetObject0D<ISurvivalProbabilitySource>(survivalProbabilitySource, "survivalProbabilitySource");
                var _date1 = XU.GetDate0D(date1, "date1");
                var _date2 = XU.GetDate0D(date2, "date2", null);
                var _result = XLCredit.GetSurvivalProb(_survivalProbabilitySource, _date1, _date2);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }
    }
}