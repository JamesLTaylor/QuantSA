using System.Collections.Generic;
using Accord.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.MarketData;
using QuantSA.General;
using QuantSA.General.Dates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Solution.Test;
using QuantSA.Valuation;
using QuantSA.Valuation.Models.CreditFX;

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
            var refEntity = TestHelpers.TestCp;
            DateGenerators.CreateDatesNoHolidays(Tenor.FromMonths(3), anchorDate, 20, out var paymentDates,
                out var accrualFractions);
            var zarNotionals = Vector.Ones(paymentDates.Length).Multiply(1000000.0);
            var zarSpreads = Vector.Ones(paymentDates.Length).Multiply(cdsSpread);
            var usdSpreads = zarSpreads.Multiply(1 + relJumpSizeInDefault); // Adjusted for the FX jump size.
            var boughtProtection = true;

            var cdsZAR = new CDS(refEntity, Currency.ZAR, paymentDates, zarNotionals, zarSpreads, accrualFractions,
                boughtProtection);
            var cdsUSD = new CDS(refEntity, Currency.USD, paymentDates, zarNotionals, usdSpreads, accrualFractions,
                boughtProtection);

            // Model
            var curveDates = new[] {anchorDate, anchorDate.AddTenor(Tenor.FromYears(10))};
            var expectedRecovery = 0.4;
            var hazardRates = new[] {cdsSpread / (1 - expectedRecovery), cdsSpread / (1 - expectedRecovery)};
            var usdRates = new[] {0.01, 0.02};
            var zarRates = new[] {0.07, 0.08};
            var usdDiscountCurve = new DatesAndRates(Currency.USD, anchorDate, curveDates, usdRates);
            var zarDiscountCurve = new DatesAndRates(Currency.ZAR, anchorDate, curveDates, zarRates);
            var abcHazardCurve = new HazardCurve(refEntity, anchorDate, curveDates, hazardRates);

            var fxSource = new FXForecastCurve(TestHelpers.USDZAR, spot, usdDiscountCurve, zarDiscountCurve);
            var fxVol = 0.15;
            var model = new DeterministicCreditWithFXJump(abcHazardCurve, TestHelpers.USDZAR, fxSource,
                zarDiscountCurve, fxVol, relJumpSizeInDefault, expectedRecovery);

            // Valuation
            var N = 5000;
            var coord = new Coordinator(model, new List<Simulator>(), N);
            var zarValue = coord.Value(new[] {cdsZAR}, anchorDate);
            var usdValue = coord.Value(new[] {cdsUSD}, anchorDate);

            Assert.AreEqual(0.0, zarValue, 800.0); // about 2bp
            Assert.AreEqual(0.0, usdValue, 800.0); // about 2bp            
        }
    }
}