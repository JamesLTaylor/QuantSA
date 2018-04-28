using System.Collections.Generic;
using Accord.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.General.Dates;
using QuantSA.Primitives.Dates;
using QuantSA.Valuation;

namespace ValuationTest
{
    [TestClass]
    public class CDSTest
    {
        [TestMethod]
        public void TestQuantoCDS()
        {
            var spot = 1.00;
            var relJumpSizeInDefault = -0.2;
            var cdsSpread = 0.025;
            // Trades
            var anchorDate = new Date(2016, 11, 25);
            var refEntity = new ReferenceEntity("ABC");
            Date[] paymentDates;
            double[] accrualFractions;
            DateGenerators.CreateDatesNoHolidays(Tenor.Months(3), anchorDate, 20, out paymentDates,
                out accrualFractions);
            var zarNotionals = Vector.Ones(paymentDates.Length).Multiply(1000000.0);
            var usdNotionals = zarNotionals.Divide(spot);
            var zarSpreads = Vector.Ones(paymentDates.Length).Multiply(cdsSpread);
            var usdSpreads = zarSpreads.Multiply(1 + relJumpSizeInDefault); // Adjusted for the FX jump size.
            var boughtProtection = true;

            var cdsZAR = new CDS(refEntity, Currency.ZAR, paymentDates, zarNotionals, zarSpreads, accrualFractions,
                boughtProtection);
            var cdsUSD = new CDS(refEntity, Currency.USD, paymentDates, zarNotionals, usdSpreads, accrualFractions,
                boughtProtection);

            // Model
            var curveDates = new[]{anchorDate, anchorDate.AddTenor(Tenor.Years(10))};
            var expectedRecovery = 0.4;
            var hazardRates = new[]{cdsSpread / (1 - expectedRecovery), cdsSpread / (1 - expectedRecovery)};
            var usdRates = new[]{0.01, 0.02};
            var zarRates = new[]{0.07, 0.08};
            IDiscountingSource usdDiscountCurve = new DatesAndRates(Currency.USD, anchorDate, curveDates, usdRates);
            IDiscountingSource zarDiscountCurve = new DatesAndRates(Currency.ZAR, anchorDate, curveDates, zarRates);
            ISurvivalProbabilitySource abcHazardCurve = new HazardCurve(refEntity, anchorDate, curveDates, hazardRates);
            var otherCurrency = Currency.USD;

            var fxSource = new FXForecastCurve(otherCurrency, Currency.ZAR, spot, usdDiscountCurve, zarDiscountCurve);
            var fxVol = 0.15;
            NumeraireSimulator model = new DeterministicCreditWithFXJump(abcHazardCurve, otherCurrency, fxSource,
                zarDiscountCurve, fxVol, relJumpSizeInDefault, expectedRecovery);

            // Valuation
            var N = 5000;
            var coord = new Coordinator(model, new List<Simulator>(), N);
            var zarValue = coord.Value(new Product[] {cdsZAR}, anchorDate);
            var usdValue = coord.Value(new Product[] {cdsUSD}, anchorDate);

            Assert.AreEqual(0.0, zarValue, 800.0); // about 2bp
            Assert.AreEqual(0.0, usdValue, 800.0); // about 2bp            
        }
    }
}