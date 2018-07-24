using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.General;
using QuantSA.General.Formulae;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Solution.Test;
using QuantSA.Valuation;
using QuantSA.Valuation.Models.Equity;

namespace ValuationTest
{
    /// <summary>
    /// Tests the compiling of a product at runtime.
    /// </summary>
    [TestClass]
    public class DynamicCompileTest
    {
        [TestMethod]
        public void TestDynamicCallFromFile()
        {
            Stopwatch watch;
            // Make a product at runtime
            var runtimeProduct = RuntimeProduct.CreateFromScript(@"ScriptEuropeanOption.txt");

            // Setup an appropriate simulation
            var shares = new[]
            {
                new Share("AAA", TestHelpers.ZAR)
            }; // One needs to know the index that will be required by the product to simulate it.
            var valueDate = new Date(2016, 08, 28);

            var divYield = new[] {0.02};
            var vol = new[] {0.22};
            var spotPrice = new[] {100.0};
            var correlations = new[,] {{1.0}};
            IDiscountingSource discountCurve = new DatesAndRates(TestHelpers.ZAR, valueDate,
                new[] {valueDate, valueDate.AddMonths(120)},
                new[] {0.07, 0.07});
            var rateForecastCurves = new IFloatingRateSource[0];

            var sim = new EquitySimulator(shares, spotPrice, vol, divYield,
                correlations, discountCurve, rateForecastCurves);

            // Value the runtime product
            Coordinator coordinator;
            coordinator = new Coordinator(sim, new List<Simulator>(), 100000);
            watch = Stopwatch.StartNew();
            var valueRuntime = coordinator.Value(new[] {runtimeProduct}, valueDate);
            watch.Stop();
            var timeRuntime = watch.ElapsedMilliseconds;

            // Setup the same product statically
            var exerciseDate = new Date(2017, 08, 28);
            var strike = 100.0;
            Product staticProduct = new EuropeanOption(new Share("AAA", TestHelpers.ZAR), strike, exerciseDate);

            // Value the static product
            coordinator = new Coordinator(sim, new List<Simulator>(), 100000);
            watch = Stopwatch.StartNew();
            var valueStatic = coordinator.Value(new[] {staticProduct}, valueDate);
            watch.Stop();
            var timeStatic = watch.ElapsedMilliseconds;

            var refValue = BlackEtc.BlackScholes(PutOrCall.Call, strike, (exerciseDate - valueDate) / 365, spotPrice[0],
                vol[0], 0.07, divYield[0]);

            Assert.AreEqual(refValue, valueRuntime, refValue * 0.03);
            Assert.AreEqual(refValue, valueStatic, refValue * 0.03);
        }


        [TestMethod]
        public void TestDynamicCallFromString()
        {
            var source =
                @"Date exerciseDate = new Date(2017, 08, 28);
Share share = new Share(""AAA"", new Currency(""ZAR""));
double strike = 100.0;

public override List<Cashflow> GetCFs()
{
    double amount = Math.Max(0, Get(share, exerciseDate) - strike);
    return new List<Cashflow>() { new Cashflow(exerciseDate, amount, share.currency) };
}";
            // Make a product at runtime
            var runtimeProduct = RuntimeProduct.CreateFromString("MyEuropeanOption", source);

            // Setup an approriate simulation
            var shares = new[]
            {
                new Share("AAA", TestHelpers.ZAR)
            }; // One needs to know the index that will be required by the product to simulate it.
            var valueDate = new Date(2016, 08, 28);

            var divYield = new[] {0.02};
            var vol = new[] {0.22};
            var spotPrice = new[] {100.0};
            var correlations = new[,] {{1.0}};
            IDiscountingSource discountCurve = new DatesAndRates(TestHelpers.ZAR, valueDate,
                new[] {valueDate, valueDate.AddMonths(120)},
                new[] {0.07, 0.07});
            var rateForecastCurves = new IFloatingRateSource[0];

            var sim = new EquitySimulator(shares, spotPrice, vol, divYield,
                correlations, discountCurve, rateForecastCurves);

            // Value the runtime product
            Coordinator coordinator;
            coordinator = new Coordinator(sim, new List<Simulator>(), 100000);
            var valueRuntime = coordinator.Value(new[] {runtimeProduct}, valueDate);

            var exerciseDate = new Date(2017, 08, 28);
            var strike = 100.0;
            var refValue = BlackEtc.BlackScholes(PutOrCall.Call, strike, (exerciseDate - valueDate) / 365, spotPrice[0],
                vol[0], 0.07, divYield[0]);

            Assert.AreEqual(refValue, valueRuntime, refValue * 0.03);
        }


        [TestMethod]
        public void TestDynamicCallFromStringFRA()
        {
            var source =
                @"Date date = new Date(2017, 08, 28);
FloatRateIndex jibar = new FloatRateIndex(""ZAR.JIBAR.3M"", new Currency(""ZAR""), ""JIBAR"", Tenor.FromMonths(3));
double dt = 91.0/365.0;
double fixedRate = 0.071;
double notional = 1000000.0;
Currency currency = new Currency(""ZAR"");

public override List<Cashflow> GetCFs()
{
    double reset = Get(jibar, date);
    double cfAmount = notional * (reset - fixedRate)*dt/(1+dt*reset);
    return new List<Cashflow>() { new Cashflow(date, cfAmount, currency) };
}";
            // Make a product at runtime
            var runtimeProduct = RuntimeProduct.CreateFromString("MyEuropeanOption", source);

            // Set up the model
            var valueDate = new Date(2016, 9, 17);
            Date[] dates = {new Date(2016, 9, 17), new Date(2026, 9, 17)};
            double[] rates = {0.07, 0.07};
            IDiscountingSource discountCurve = new DatesAndRates(TestHelpers.ZAR, valueDate, dates, rates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, TestHelpers.Jibar3M, dates, rates);
            var curveSim = new DeterminsiticCurves(discountCurve);
            curveSim.AddRateForecast(forecastCurve);

            // Run the valuation
            var coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);
            var fraValue = coordinator.Value(new[] {runtimeProduct}, valueDate);

            var date = new Date(2017, 08, 28);
            var t = (date - valueDate) / 365.0;
            var dt = 91.0 / 365.0;
            var fixedRate = 0.071;
            var notional = 1000000.0;
            var fwdRate = 0.07;
            var refValue = notional * (fwdRate - fixedRate) * dt / (1 + fwdRate * dt) * Math.Exp(-t * 0.07);

            Assert.AreEqual(refValue, fraValue, 0.01);
        }
    }
}