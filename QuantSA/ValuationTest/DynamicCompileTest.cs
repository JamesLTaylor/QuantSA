using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using QuantSA.Valuation;
using System;
using QuantSA.Primitives;
using QuantSA.Primitives.Curves;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.Formulae;
using QuantSA.Primitives.MarketObservables;
using QuantSA.Primitives.Products;
using QuantSA.Primitives.Products.Equity;
using QuantSA.Valuation.Models.Equity;
using QuantSA.Valuation.Models.Rates;

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
            Product runtimeProduct = RuntimeProduct.CreateFromSourceFile(@"ScriptEuropeanOption.txt");

            // Setup an approriate simulation
            Share[] shares = new Share[] { new Share("AAA", Currency.ZAR) };// One needs to know the index that will be required by the product to simulate it.
            Date valueDate = new Date(2016, 08, 28);
            
            double[] divYield = new double[] { 0.02 };
            double[] vol = new double[] { 0.22 };
            double[] spotPrice = new double[] { 100.0 };
            double[,] correlations = new double[,] { { 1.0 } };
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate,
                new Date[] { valueDate, valueDate.AddMonths(120) },
                new double[] { 0.07, 0.07 });
            IFloatingRateSource[] rateForecastCurves = new IFloatingRateSource[0];

            EquitySimulator sim = new EquitySimulator(shares, spotPrice, vol, divYield, 
                correlations, discountCurve, rateForecastCurves);
            
            // Value the runtime product
            Coordinator coordinator;
            coordinator = new Coordinator(sim, new List<Simulator>(), 100000);
            watch = Stopwatch.StartNew();
            double valueRuntime = coordinator.Value(new Product[] { runtimeProduct }, valueDate);
            watch.Stop();
            long timeRuntime = watch.ElapsedMilliseconds;

            // Setup the same product statically
            Date exerciseDate = new Date(2017, 08, 28);            
            double strike = 100.0;
            Product staticProduct = new EuropeanOption(new Share("AAA", Currency.ZAR), strike, exerciseDate);

            // Value the static product
            coordinator = new Coordinator(sim, new List<Simulator> (), 100000);
            watch = Stopwatch.StartNew();
            double valueStatic = coordinator.Value(new Product[] { staticProduct }, valueDate);
            watch.Stop();
            long timeStatic = watch.ElapsedMilliseconds;

            double refValue = BlackEtc.BlackScholes(PutOrCall.Call, strike, (exerciseDate - valueDate) / 365, spotPrice[0],
                                                    vol[0], 0.07, divYield[0]);

            Assert.AreEqual(refValue, valueRuntime, refValue * 0.03);
            Assert.AreEqual(refValue, valueStatic, refValue * 0.03);

        }


        [TestMethod]
        public void TestDynamicCallFromString()
        {
            string source =
@"Date exerciseDate = new Date(2017, 08, 28);
Share share = new Share(""AAA"", Currency.ZAR);
double strike = 100.0;

public override List<Cashflow> GetCFs()
{
    double amount = Math.Max(0, Get(share, exerciseDate) - strike);
    return new List<Cashflow>() { new Cashflow(exerciseDate, amount, share.currency) };
}";
            // Make a product at runtime
            Product runtimeProduct = RuntimeProduct.CreateFromString("MyEuropeanOption", source);

            // Setup an approriate simulation
            Share[] shares = new Share[] { new Share("AAA", Currency.ZAR) };// One needs to know the index that will be required by the product to simulate it.
            Date valueDate = new Date(2016, 08, 28);

            double[] divYield = new double[] { 0.02 };
            double[] vol = new double[] { 0.22 };
            double[] spotPrice = new double[] { 100.0 };
            double[,] correlations = new double[,] { { 1.0 } };
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate,
                new Date[] { valueDate, valueDate.AddMonths(120) },
                new double[] { 0.07, 0.07 });
            IFloatingRateSource[] rateForecastCurves = new IFloatingRateSource[0];

            EquitySimulator sim = new EquitySimulator(shares, spotPrice, vol, divYield,
                correlations, discountCurve, rateForecastCurves);

            // Value the runtime product
            Coordinator coordinator;
            coordinator = new Coordinator(sim, new List<Simulator>(), 100000);
            double valueRuntime = coordinator.Value(new Product[] { runtimeProduct }, valueDate);

            Date exerciseDate = new Date(2017, 08, 28);
            double strike = 100.0;
            double refValue = BlackEtc.BlackScholes(PutOrCall.Call, strike, (exerciseDate - valueDate) / 365, spotPrice[0],
                                                    vol[0], 0.07, divYield[0]);

            Assert.AreEqual(refValue, valueRuntime, refValue * 0.03);
        }


        [TestMethod]
        public void TestDynamicCallFromStringFRA()
        {
            string source =
@"Date date = new Date(2017, 08, 28);
FloatingIndex jibar = FloatingIndex.JIBAR3M;
double dt = 91.0/365.0;
double fixedRate = 0.071;
double notional = 1000000.0;
Currency currency = Currency.ZAR;

public override List<Cashflow> GetCFs()
{
    double reset = Get(jibar, date);
    double cfAmount = notional * (reset - fixedRate)*dt/(1+dt*reset);
    return new List<Cashflow>() { new Cashflow(date, cfAmount, currency) };
}";
            // Make a product at runtime
            Product runtimeProduct = RuntimeProduct.CreateFromString("MyEuropeanOption", source);

            // Set up the model
            Date valueDate = new Date(2016, 9, 17);
            Date[] dates = { new Date(2016, 9, 17), new Date(2026, 9, 17) };
            double[] rates = { 0.07, 0.07 };
            IDiscountingSource discountCurve = new DatesAndRates(Currency.ZAR, valueDate, dates, rates);
            IFloatingRateSource forecastCurve = new ForecastCurve(valueDate, FloatingIndex.JIBAR3M, dates, rates);
            DeterminsiticCurves curveSim = new DeterminsiticCurves(discountCurve);
            curveSim.AddRateForecast(forecastCurve);

            // Run the valuation
            Coordinator coordinator = new Coordinator(curveSim, new List<Simulator>(), 1);
            double fraValue = coordinator.Value(new Product[] { runtimeProduct }, valueDate);

            Date date = new Date(2017, 08, 28);
            double t = (date - valueDate) / 365.0;
            double dt = 91.0 / 365.0;
            double fixedRate = 0.071;
            double notional = 1000000.0;
            double fwdRate = 0.07;
            double refValue = notional * (fwdRate - fixedRate) * dt / (1 + fwdRate * dt) * Math.Exp(-t * 0.07);

            Assert.AreEqual(refValue, fraValue, 0.01);
        }

    }
}
