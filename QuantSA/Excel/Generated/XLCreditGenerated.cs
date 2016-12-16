using System;
using XU = QuantSA.Excel.ExcelUtilities;
using QuantSA.General;
using QuantSA.Valuation;
using QuantSA.ExcelFunctions;

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
                ReferenceEntity _refEntity = XU.GetReferenceEntity0D(refEntity, "refEntity");
                Currency _ccy = XU.GetCurrency0D(ccy, "ccy");
                Date[] _paymentDates = XU.GetDate1D(paymentDates, "paymentDates");
                Double[] _notionals = XU.GetDouble1D(notionals, "notionals");
                Double[] _rates = XU.GetDouble1D(rates, "rates");
                Double[] _accrualFractions = XU.GetDouble1D(accrualFractions, "accrualFractions");
                Boolean _boughtProtection = XU.GetBoolean0D(boughtProtection, "boughtProtection");
                CDS _result = XLCredit.CreateCDS(_refEntity, _ccy, _paymentDates, _notionals, _rates, _accrualFractions, _boughtProtection);
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
                ISurvivalProbabilitySource _survivalProbSource = XU.GetObject0D<ISurvivalProbabilitySource>(survivalProbSource, "survivalProbSource");
                Currency _otherCurrency = XU.GetCurrency0D(otherCurrency, "otherCurrency");
                IFXSource _fxSource = XU.GetObject0D<IFXSource>(fxSource, "fxSource");
                IDiscountingSource _valueCurrencyDiscount = XU.GetObject0D<IDiscountingSource>(valueCurrencyDiscount, "valueCurrencyDiscount");
                Double _fxVol = XU.GetDouble0D(fxVol, "fxVol");
                Double _relJumpSizeInDefault = XU.GetDouble0D(relJumpSizeInDefault, "relJumpSizeInDefault");
                Double _expectedRecoveryRate = XU.GetDouble0D(expectedRecoveryRate, "expectedRecoveryRate");
                DeterministicCreditWithFXJump _result = XLCredit.CreateModelDeterministicCreditWithFXJump(_survivalProbSource, _otherCurrency, _fxSource, _valueCurrencyDiscount, _fxVol, _relJumpSizeInDefault, _expectedRecoveryRate);
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
                ReferenceEntity _referenceEntity = XU.GetReferenceEntity0D(referenceEntity, "referenceEntity");
                Date _anchorDate = XU.GetDate0D(anchorDate, "anchorDate");
                Date[] _dates = XU.GetDate1D(dates, "dates");
                Double[] _hazardRates = XU.GetDouble1D(hazardRates, "hazardRates");
                HazardCurve _result = XLCredit.CreateHazardCurve(_referenceEntity, _anchorDate, _dates, _hazardRates);
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
                ISurvivalProbabilitySource _survivalProbabilitySource = XU.GetObject0D<ISurvivalProbabilitySource>(survivalProbabilitySource, "survivalProbabilitySource");
                Date _date1 = XU.GetDate0D(date1, "date1");
                Date _date2 = XU.GetDate0D(date2, "date2", null);
                Double _result = XLCredit.GetSurvivalProb(_survivalProbabilitySource, _date1, _date2);
                return XU.ConvertToObjects(_result);
            }
            catch (Exception e)
            {
                return XU.Error0D(e);
            }
        }

    }
}
