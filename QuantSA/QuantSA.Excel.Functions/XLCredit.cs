using QuantSA.Excel.Shared;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation;

namespace QuantSA.ExcelFunctions
{
    public class XLCredit
    {
        [QuantSAExcelFunction(
            Description =
                "Create a par style CDS.  Protection will always apply from the value date until the last payment date.",
            Name = "QSA.CreateCDS",
            HasGeneratedVersion = true,
            Category = "QSA.Credit",
            ExampleSheet = "CDS.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateCDS.html")]
        public static CDS CreateCDS(
            [QuantSAExcelArgument(Description = "The reference entity whose default is covered by this CDS.")]
            ReferenceEntity refEntity,
            [QuantSAExcelArgument(Description = "The currency of the cashflows of the premium and default legs.")]
            Currency ccy,
            [QuantSAExcelArgument(Description = "The payment dates on which the premium is paid.")]
            Date[] paymentDates,
            [QuantSAExcelArgument(Description =
                "The notionals that define the protection amount in the period until each " +
                "payment date and the basis on which the premiums are calculated.")]
            double[] notionals,
            [QuantSAExcelArgument(Description = "The simple rates that apply until the default time.  " +
                                                "Used to calculate the premium flows.")]
            double[] rates,
            [QuantSAExcelArgument(Description = "The accrual fractions used to calculate the premiums paid on the " +
                                                "paymentDates.")]
            double[] accrualFractions,
            [QuantSAExcelArgument(Description =
                "If set to TRUE then protection has been bought and the premium will be paid.")]
            bool boughtProtection)
        {
            return new CDS(refEntity, ccy, paymentDates, notionals, rates, accrualFractions, boughtProtection);
        }

        [QuantSAExcelFunction(
            Description =
                "Create a model that will simulate a single FX process and default for a single reference entity.",
            Name = "QSA.CreateModelDeterministicCreditWithFXJump",
            HasGeneratedVersion = true,
            Category = "QSA.Credit",
            ExampleSheet = "CDS.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateModelDeterministicCreditWithFXJump.html")]
        public static DeterministicCreditWithFXJump CreateModelDeterministicCreditWithFXJump(
            [QuantSAExcelArgument(Description =
                "A curve that provides survival probabilities.  Usually a hazard curve.")]
            ISurvivalProbabilitySource survivalProbSource,
            [QuantSAExcelArgument(Description =
                "The other currency required in the simulation.  The valuation currency will be inferred from the valueCurrencyDiscount.  This value needs to be explicitly set since fxSource may provide multiple pairs.")]
            Currency otherCurrency,
            [QuantSAExcelArgument(Description = "The source FX spot and forwards.")]
            IFXSource fxSource,
            [QuantSAExcelArgument(Description = "The value currency discount curve.")]
            IDiscountingSource valueCurrencyDiscount,
            [QuantSAExcelArgument(Description = "The fx volatility.")]
            double fxVol,
            [QuantSAExcelArgument(Description =
                "The relative jump size in default.  For example if the value currency is ZAR and the other currency is USD then the fx is modelled as ZAR per USD and in default the fx rate will change to: rate before default * (1 + relJumpSizeInDefault).")]
            double relJumpSizeInDefault,
            [QuantSAExcelArgument(Description = "The constant recovery rate that will be assumed to apply in default.")]
            double expectedRecoveryRate)
        {
            return new DeterministicCreditWithFXJump(survivalProbSource,
                otherCurrency, fxSource, valueCurrencyDiscount,
                fxVol, relJumpSizeInDefault, expectedRecoveryRate);
        }

        [QuantSAExcelFunction(
            Description =
                "Create hazard rate curve that can be used to provide survival probabilities for a reference entity between dates.",
            Name = "QSA.CreateHazardCurve",
            HasGeneratedVersion = true,
            Category = "QSA.Credit",
            ExampleSheet = "CDS.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/CreateHazardCurve.html")]
        public static HazardCurve CreateHazardCurve(
            [QuantSAExcelArgument(Description = "The reference entity for whom these hazard rates apply.")]
            ReferenceEntity referenceEntity,
            [QuantSAExcelArgument(Description =
                "The anchor date.  Survival probabilities can only be calculated up to dates after this date.")]
            Date anchorDate,
            [QuantSAExcelArgument(Description = "The dates on which the hazard rates apply.")]
            Date[] dates,
            [QuantSAExcelArgument(Description = "The hazard rates.")]
            double[] hazardRates)
        {
            return new HazardCurve(referenceEntity, anchorDate, dates, hazardRates);
        }

        [QuantSAExcelFunction(
            Description = "Get the survival probability from the anchor date until a date or between two dates.",
            Name = "QSA.GetSurvivalProb",
            HasGeneratedVersion = true,
            Category = "QSA.Credit",
            ExampleSheet = "CDS.xlsx",
            IsHidden = false,
            HelpTopic = "http://www.quantsa.org/GetSurvivalProb.html")]
        public static double GetSurvivalProb(
            [QuantSAExcelArgument(Description = "The hazard rate curve or other source of default probabilities.")]
            ISurvivalProbabilitySource survivalProbabilitySource,
            [QuantSAExcelArgument(Description =
                "If date2 is omitted the date until which survival is calculated.  If date2 is provided the date from which survival is calculated.")]
            Date date1,
            [QuantSAExcelArgument(
                Description =
                    "Optional: If provided then the survival probability is calculated from date1 until date2.",
                Default = null)]
            Date date2)
        {
            return date2 == null
                ? survivalProbabilitySource.GetSP(date1)
                : survivalProbabilitySource.GetSP(date1, date2);
        }
    }
}