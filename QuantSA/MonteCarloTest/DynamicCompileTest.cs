using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using General;
using MonteCarlo;
using MonteCarlo.Equity;
using MonteCarlo.Rates;
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
            Simulator sim = new SimpleBlackEquity(valueDate, shareCode, spotPrice, vol, riskfreeRate, divYield);
            NumeraireSimulator numeraire = new DeterministicNumeraire(new Currency("ZAR"), valueDate, riskfreeRate);

            // Value the runtime product
            Coordinator coordinator;
            coordinator = new Coordinator(numeraire, new List<Product> { runtimeProduct }, new List<Simulator> { sim });
            watch = Stopwatch.StartNew();
            double valueRuntime = coordinator.Value(valueDate);
            watch.Stop();
            long timeRuntime = watch.ElapsedMilliseconds;

            // Setup the same product statically
            Date exerciseDate = new Date(2017, 08, 28);            
            double strike = 100.0;
            Product staticProduct = new EuropeanOption(shareCode, strike, exerciseDate);

            // Value the static product
            coordinator = new Coordinator(numeraire, new List<Product> { staticProduct }, new List<Simulator> { sim });
            watch = Stopwatch.StartNew();
            double valueStatic = coordinator.Value(valueDate);
            watch.Stop();
            long timeStatic = watch.ElapsedMilliseconds;

            double refValue = Formulae.BlackScholes(PutOrCall.Call, strike, (exerciseDate - valueDate) / 365, spotPrice,
                                                    vol, riskfreeRate, divYield);

            Assert.AreEqual(refValue, valueRuntime, refValue * 0.01);
            Assert.AreEqual(refValue, valueStatic, refValue * 0.01);

        }
    }
}
