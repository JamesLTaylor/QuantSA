using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA;
using MonteCarlo;
using System.Diagnostics;

namespace MonteCarloTest
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
            Product runtimeProduct = RuntimeProduct.CreateFromScript(@"C:\Dev\QuantSA\Scripts\EuropeanOption.txt");

            // Setup an approriate simulation
            string shareCode = "AAA"; // One needs to know the index that will be required by the product to simulate it.
            Date valueDate = new Date(2016, 08, 28);
            double riskfreeRate = 0.07;
            double divYield = 0.02;
            double vol = 0.22;
            double spotPrice = 100.0;
            Simulator sim = new SimpleBlackEquity(valueDate, new Share(shareCode, Currency.ZAR), spotPrice, vol, riskfreeRate, divYield);
            NumeraireSimulator numeraire = new DeterministicNumeraire(Currency.ZAR, valueDate, riskfreeRate);

            // Value the runtime product
            Coordinator coordinator;
            coordinator = new Coordinator(numeraire, new List<Simulator> { sim }, 100000);
            watch = Stopwatch.StartNew();
            double valueRuntime = coordinator.Value(new List<Product> { runtimeProduct }, valueDate);
            watch.Stop();
            long timeRuntime = watch.ElapsedMilliseconds;

            // Setup the same product statically
            Date exerciseDate = new Date(2017, 08, 28);            
            double strike = 100.0;
            Product staticProduct = new EuropeanOption(new Share(shareCode, Currency.ZAR), strike, exerciseDate);

            // Value the static product
            coordinator = new Coordinator(numeraire, new List<Simulator> { sim }, 100000);
            watch = Stopwatch.StartNew();
            double valueStatic = coordinator.Value(new List<Product> { staticProduct }, valueDate);
            watch.Stop();
            long timeStatic = watch.ElapsedMilliseconds;

            double refValue = Formulae.BlackScholes(PutOrCall.Call, strike, (exerciseDate - valueDate) / 365, spotPrice,
                                                    vol, riskfreeRate, divYield);

            Assert.AreEqual(refValue, valueRuntime, refValue * 0.02);
            Assert.AreEqual(refValue, valueStatic, refValue * 0.02);

        }
    }
}
