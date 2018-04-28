using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.General;
using QuantSA.Primitives.Dates;
using QuantSA.Valuation;
using QuantSA.Valuation.Models;

namespace ValuationTest
{
    [TestClass]
    public class MultiHWAndFXToyTest
    {
        [TestMethod]
        public void TestMultiHWAndFXToyForwards()
        {
            var valueDate = new Date(2016, 9, 17);
            var zarRatesSim = new HullWhite1F(Currency.ZAR, 0.05, 0.01, 0.07, 0.07, valueDate);
            zarRatesSim.AddForecast(FloatingIndex.JIBAR3M);
            var usdRatesSim = new HullWhite1F(Currency.USD, 0.05, 0.0000001, 0.01, 0.01, valueDate);
            usdRatesSim.AddForecast(FloatingIndex.LIBOR3M);
            var eurRatesSim = new HullWhite1F(Currency.EUR, 0.05, 0.0000001, 0.005, 0.005, valueDate);
            eurRatesSim.AddForecast(FloatingIndex.EURIBOR3M);

            CurrencyPair[] currencyPairs =
                {new CurrencyPair(Currency.USD, Currency.ZAR), new CurrencyPair(Currency.EUR, Currency.ZAR)};
            double[] spots = {13.6, 15.0};
            double[] vols = {0.15, 0.15};
            double[,] correlations = {{1.0, 0.0}, {0.0, 1.0}};
            var model = new MultiHWAndFXToy(valueDate, Currency.ZAR, new[] {zarRatesSim, usdRatesSim, eurRatesSim},
                currencyPairs, spots, vols, correlations);

            var simDates = new List<Date>();
            simDates.Add(valueDate.AddMonths(24));
            model.Reset();
            model.SetNumeraireDates(simDates);
            model.SetRequiredDates(currencyPairs[0], simDates); // Will simulate both currency pairs
            model.Prepare();

            var N = 100;
            var fwdSpotValues = Matrix.Zeros(N, 5);

            for (var i = 0; i < N; i++)
            {
                model.RunSimulation(i);
                fwdSpotValues[i, 0] = model.GetIndices(currencyPairs[0], simDates)[0];
                fwdSpotValues[i, 1] = model.GetIndices(currencyPairs[1], simDates)[0];
                fwdSpotValues[i, 2] = model.GetIndices(FloatingIndex.JIBAR3M, simDates)[0];
                fwdSpotValues[i, 3] = model.GetIndices(FloatingIndex.LIBOR3M, simDates)[0];
                fwdSpotValues[i, 4] = model.GetIndices(FloatingIndex.EURIBOR3M, simDates)[0];
            }

            var meanUSDZAR = fwdSpotValues.GetColumn(0).Mean();
            var meanEURZAR = fwdSpotValues.GetColumn(1).Mean();
            var meanJibar = fwdSpotValues.GetColumn(2).Mean();
            var meanLibor = fwdSpotValues.GetColumn(3).Mean();
            var meanEuribor = fwdSpotValues.GetColumn(4).Mean();
            Assert.AreEqual(15.7, meanUSDZAR, 0.1);
            Assert.AreEqual(17.2, meanEURZAR, 0.1);
            Assert.AreEqual(0.071, meanJibar, 1e-4);
            Assert.AreEqual(0.01, meanLibor, 1e-4);
            Assert.AreEqual(0.005, meanEuribor, 1e-4);
        }


        private FloatLeg CreateFloatingLeg(Currency ccy, Date startDate, double notional, FloatingIndex index,
            int tenorYears)
        {
            var quarters = tenorYears * 4;
            var paymentDates = Enumerable.Range(1, quarters).Select(i => startDate.AddMonths(3 * i)).ToArray();
            var resetDates = Enumerable.Range(0, quarters).Select(i => startDate.AddMonths(3 * i)).ToArray();
            var notionals = Vector.Ones(quarters).Multiply(notional);
            var spreads = Vector.Zeros(quarters);
            var accrualFractions = Vector.Ones(quarters).Multiply(0.25);
            var floatingIndices = Enumerable.Range(1, quarters).Select(i => index).ToArray();
            var leg = new FloatLeg(ccy, paymentDates, notionals, resetDates, floatingIndices, spreads,
                accrualFractions);
            return leg;
        }

        /// <summary>
        /// Tests the <see cref="MultiHWAndFXToy"/> with respect to generating PFEs on a portfolio of CCIRSs
        /// </summary>
        [TestMethod]
        public void TestMultiHWAndFXToyCCIRS()
        {
            var valueDate = new Date(2016, 9, 17);
            var zarRatesSim = new HullWhite1F(Currency.ZAR, 0.05, 0.01, 0.07, 0.07, valueDate);
            zarRatesSim.AddForecast(FloatingIndex.JIBAR3M);
            var usdRatesSim = new HullWhite1F(Currency.USD, 0.05, 0.01, 0.01, 0.01, valueDate);
            usdRatesSim.AddForecast(FloatingIndex.LIBOR3M);
            var eurRatesSim = new HullWhite1F(Currency.EUR, 0.05, 0.01, 0.005, 0.005, valueDate);
            eurRatesSim.AddForecast(FloatingIndex.EURIBOR3M);

            var currencyPairs = new[]
                {new CurrencyPair(Currency.USD, Currency.ZAR), new CurrencyPair(Currency.EUR, Currency.ZAR)};
            var spots = new[] { 13.6, 15.0};
            var vols = new[] { 0.15, 0.15};
            var correlations = new[,]
            {
                {1.0, 0.0},
                {0.0, 1.0}
            };
            var model = new MultiHWAndFXToy(valueDate, Currency.ZAR, new[] {zarRatesSim, usdRatesSim, eurRatesSim},
                currencyPairs, spots, vols, correlations);

            var portfolio = new List<Product>();
            portfolio.Add(CreateFloatingLeg(Currency.ZAR, valueDate, -15e6, FloatingIndex.JIBAR3M, 7));
            portfolio.Add(CreateFloatingLeg(Currency.EUR, valueDate, +1e6, FloatingIndex.EURIBOR3M, 7));
            portfolio.Add(CreateFloatingLeg(Currency.ZAR, valueDate, 13e6, FloatingIndex.JIBAR3M, 13));
            portfolio.Add(CreateFloatingLeg(Currency.USD, valueDate, -1e6, FloatingIndex.EURIBOR3M, 13));
            portfolio.Add(IRSwap.CreateZARSwap(0.07, true, 20e6, valueDate, Tenor.Years(4)));

            var stepInMonths = 1;
            var fwdValueDates = Enumerable.Range(1, 13 * 12 / stepInMonths)
                .Select(i => valueDate.AddMonths(stepInMonths * i)).ToArray();
            var coord = new Coordinator(model, new List<Simulator>(), 1000);
            //coord.SetThreadedness(false);
            var epe = coord.EPE(portfolio.ToArray(), valueDate, fwdValueDates);
            Assert.AreEqual(1555002, epe[0], 5000);
            Assert.AreEqual(2170370, epe[87], 5000);
            Assert.AreEqual(0, epe[155], 5);

            //Debug.WriteToFile("c:\\dev\\quantsa\\temp\\epeTest_singlethread_10000.csv", epe);
        }

        /// <summary>
        /// Tests the <see cref="MultiHWAndFXToy"/> with respect to generating PFEs on a portfolio of CCIRSs
        /// </summary>
        [TestMethod]
        public void TestMultiHWAndFXToyCCIRSAltConstructor()
        {
            var valueDate = new Date(2016, 9, 17);
            var spots = new List<double> {13.6, 15.0};
            var vols = new List<double> {0.15, 0.15};
            var correlations = new[,]
            {
                {1.0, 0.0},
                {0.0, 1.0}
            };
            // ZAR HW specs
            IDiscountingSource zarCurve = new DatesAndRates(Currency.ZAR, valueDate,
                new[] {valueDate, valueDate.AddMonths(240)},
                new[] {0.07, 0.07});
            var zarHWParams = new HWParams {vol = 0.01, meanReversionSpeed = 0.05};
            var zarRequiredIndices = new List<FloatingIndex> {FloatingIndex.JIBAR3M};

            // Lists to be populated for other currencies
            var otherCcys = new List<Currency>();
            var otherCcyCurves = new List<IDiscountingSource>();
            var otherCcyHwParams = new List<HWParams>();
            var otherCcyRequiredIndices = new List<List<FloatingIndex>>();

            // USD HW specs
            otherCcys.Add(Currency.USD);
            otherCcyCurves.Add(new DatesAndRates(Currency.USD, valueDate, new[] {valueDate, valueDate.AddMonths(240)},
                new[] {0.01, 0.01}));
            otherCcyHwParams.Add(new HWParams {vol = 0.01, meanReversionSpeed = 0.05});
            otherCcyRequiredIndices.Add(new List<FloatingIndex> {FloatingIndex.LIBOR3M});

            // EUR HW specs
            otherCcys.Add(Currency.EUR);
            otherCcyCurves.Add(new DatesAndRates(Currency.EUR, valueDate, new[] {valueDate, valueDate.AddMonths(240)},
                new[] {0.005, 0.005}));
            otherCcyHwParams.Add(new HWParams {vol = 0.01, meanReversionSpeed = 0.05});
            otherCcyRequiredIndices.Add(new List<FloatingIndex> {FloatingIndex.EURIBOR3M});

            // Construct the model
            var model = new MultiHWAndFXToy(valueDate, zarCurve, zarRequiredIndices, zarHWParams,
                otherCcys, spots, vols, otherCcyCurves, otherCcyRequiredIndices, otherCcyHwParams, correlations);

            var portfolio = new List<Product>();
            portfolio.Add(CreateFloatingLeg(Currency.ZAR, valueDate, -15e6, FloatingIndex.JIBAR3M, 7));
            portfolio.Add(CreateFloatingLeg(Currency.EUR, valueDate, +1e6, FloatingIndex.EURIBOR3M, 7));
            portfolio.Add(CreateFloatingLeg(Currency.ZAR, valueDate, 13e6, FloatingIndex.JIBAR3M, 13));
            portfolio.Add(CreateFloatingLeg(Currency.USD, valueDate, -1e6, FloatingIndex.EURIBOR3M, 13));
            portfolio.Add(IRSwap.CreateZARSwap(0.07, true, 20e6, valueDate, Tenor.Years(4)));

            var stepInMonths = 1;
            var fwdValueDates = Enumerable.Range(1, 13 * 12 / stepInMonths)
                .Select(i => valueDate.AddMonths(stepInMonths * i)).ToArray();
            var coord = new Coordinator(model, new List<Simulator>(), 1000);
            //coord.SetThreadedness(false);
            var epe = coord.EPE(portfolio.ToArray(), valueDate, fwdValueDates);
            Assert.AreEqual(1555002, epe[0], 5000);
            Assert.AreEqual(2170370, epe[87], 5000);
            Assert.AreEqual(0, epe[155], 5);

            //Debug.WriteToFile("c:\\dev\\quantsa\\temp\\epeTest_2.csv", epe);
        }
    }
}