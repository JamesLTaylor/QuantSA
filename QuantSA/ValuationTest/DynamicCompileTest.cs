using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using QuantSA.General;
using QuantSA.Valuation;

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
            Product runtimeProduct = RuntimeProduct.CreateFromScript(@"C:\Dev\QuantSA\Scripts\EuropeanOption.cs");

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

            double refValue = Formulae.BlackScholes(PutOrCall.Call, strike, (exerciseDate - valueDate) / 365, spotPrice[0],
                                                    vol[0], 0.07, divYield[0]);

            Assert.AreEqual(refValue, valueRuntime, refValue * 0.03);
            Assert.AreEqual(refValue, valueStatic, refValue * 0.03);

        }
    }
}
