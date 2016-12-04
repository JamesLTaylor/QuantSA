using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using Accord.Math;
using QuantSA.Valuation;
using System.Collections.Generic;

namespace ValuationTest
{
    [TestClass]
    public class CDSTest
    {
        [TestMethod]
        public void TestQuantoCDS()
        {
            double spot = 1.00;
            double relJumpSizeInDefault = -0.2;
            double cdsSpread = 0.025;
            // Trades
            Date anchorDate = new Date(2016, 11, 25);
            ReferenceEntity refEntity = new ReferenceEntity("ABC");
            Date[] paymentDates;
            double[] accrualFractions;
            DateGenerators.CreateDatesNoHolidays(Tenor.Months(3), anchorDate, 20, out paymentDates, out accrualFractions);
            double[] zarNotionals = Vector.Ones(paymentDates.Length).Multiply(1000000.0);
            double[] usdNotionals = zarNotionals.Divide(spot);
            double[] zarSpreads = Vector.Ones(paymentDates.Length).Multiply(cdsSpread);
            double[] usdSpreads = zarSpreads.Multiply((1+ relJumpSizeInDefault)); // Adjusted for the FX jump size.
            bool boughtProtection = true;
            
            CDS cdsZAR = new CDS(refEntity, Currency.ZAR, paymentDates, zarNotionals, zarSpreads, accrualFractions, boughtProtection);
            CDS cdsUSD = new CDS(refEntity, Currency.USD, paymentDates, zarNotionals, usdSpreads, accrualFractions, boughtProtection);

            // Model
            Date[] curveDates = new Date[] { anchorDate,  anchorDate.AddTenor(Tenor.Years(10))};
            double expectedRecovery = 0.4;
            double[] hazardRates = new double[] { cdsSpread / (1-expectedRecovery), cdsSpread / (1 - expectedRecovery) };
            double[] usdRates = new double[] { 0.01, 0.02 };
            double[] zarRates = new double[] { 0.07, 0.08 };
            IDiscountingSource usdDiscountCurve = new DatesAndRates(Currency.USD, anchorDate, curveDates, usdRates);
            IDiscountingSource zarDiscountCurve = new DatesAndRates(Currency.ZAR, anchorDate, curveDates, zarRates);
            ISurvivalProbabilitySource abcHazardCurve = new HazardCurve(refEntity, anchorDate, curveDates, hazardRates);
            Currency otherCurrency = Currency.USD;
            
            FXForecastCurve fxSource = new FXForecastCurve(otherCurrency, Currency.ZAR, spot, usdDiscountCurve, zarDiscountCurve);
            double fxVol = 0.15;            
            NumeraireSimulator model = new DeterministicCreditWithFXJump(abcHazardCurve, otherCurrency, fxSource,
                zarDiscountCurve, fxVol, relJumpSizeInDefault, expectedRecovery);

            // Valuation
            int N = 5000;
            Coordinator coord = new Coordinator(model, new List<Simulator>(), N);
            double zarValue = coord.Value(new Product[] { cdsZAR }, anchorDate);
            double usdValue = coord.Value(new Product[] { cdsUSD }, anchorDate);

            Assert.AreEqual(0.0, zarValue, 800.0); // about 2bp
            Assert.AreEqual(0.0, usdValue, 800.0); // about 2bp            

        }
    }
}
