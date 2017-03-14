using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using QuantSA.General;
using QuantSA.Valuation;
using Accord.Math;
using Accord.Statistics;
using QuantSA.General.Formulae;

namespace ValuationTest
{
    /// <summary>
    /// Sample product for test simulated indices.
    /// </summary>
    [Serializable]
    public class ProductWithDiviAndFwd : ProductWrapper
    {
        Date dealStartDate = new Date(2016, 9, 30); // The issue date of the scheme
        Date dealEndDate = new Date(2019, 9, 30);        
        double nShares = 1;
        Share share = new Share("AAA", Currency.ZAR);
        Dividend dividend = new Dividend(new Share("AAA", Currency.ZAR));
        FloatingIndex jibar = FloatingIndex.JIBAR3M;

        public override List<Cashflow> GetCFs()
        {
            double loanBalance = 40; // Opening balance on startDate
            double spread = 0.035 + 0.02; // 350bp prime Jibar spread + 200bp deal spread

            Date periodStartDate = dealStartDate;
            Date periodEndDate = dealStartDate.AddMonths(3);
            
            List<Cashflow> cfs = new List<Cashflow>();
            // Generate the dividend cashflows.
            while (periodEndDate<=dealEndDate)
            {
                double observedRate = Get(jibar, periodStartDate);
                if (loanBalance < 1e-6)
                {
                    cfs.Add(new Cashflow(new Date(periodEndDate), 0, Currency.ZAR));
                }
                else
                {
                    loanBalance *= (1 + (observedRate + spread) * 0.25);
                    double loanReduction = 0.9 * nShares * Get(dividend, periodEndDate);
                    if (loanReduction > loanBalance)
                        cfs.Add(new Cashflow(new Date(periodEndDate), loanBalance, Currency.ZAR));
                    else
                        cfs.Add(new Cashflow(new Date(periodEndDate), loanReduction, Currency.ZAR));
                    loanBalance = Math.Max(0, loanBalance - loanReduction);
                }
                periodStartDate = new Date(periodEndDate);
                periodEndDate = periodEndDate.AddMonths(3);
            }
            // Check if the loan is worth more than the shares.  If yes then there is a loss on the loan, 
            // otherwise the loan can be repaid in full.
            double finalPayment = Math.Min(loanBalance, nShares * Get(share, dealEndDate));
            cfs.Add(new Cashflow(new Date(dealEndDate), finalPayment, Currency.ZAR));

            return cfs;
        }
    }

    /// <summary>
    /// Test class for <see cref="EquitySimulator"/>
    /// </summary>
    [TestClass]
    public class EquitySimulatorTest
    {
        Date anchorDate = new Date(2016, 09, 30);
        Share[] shares = new Share[] { new Share("ALSI", Currency.ZAR), new Share("AAA", Currency.ZAR), new Share("BBB", Currency.ZAR) };
        double[] prices = { 200, 50, 100 };
        double[] vols = { 0.22, 0.52, 0.4};
        double[] divYields = { 0.03, 0.0, 0.0 };
        double[,] correlations = { { 1.0, 0.4, 0.5},
                                    {0.4, 1.0, 0.6 },
                                    {0.5, 0.6, 1.0 }};
        IDiscountingSource discountCurve;
        IFloatingRateSource[] rateForecastCurves;

        [TestInitialize]
        public void Init()
        {
            discountCurve = new DatesAndRates(Currency.ZAR, anchorDate,
                new Date[] { anchorDate, anchorDate.AddMonths(120) },
                new double[] { 0.07, 0.09 });
            rateForecastCurves = new List<IFloatingRateSource>
                { new ForecastCurveFromDiscount(discountCurve, FloatingIndex.JIBAR3M, new FloatingRateFixingCurve1Rate(0.07, FloatingIndex.JIBAR3M)) }.ToArray();
        }

        [TestMethod]
        public void TestEquitySimulatorDivis()
        {
            EquitySimulator sim = new EquitySimulator(shares, prices, vols, divYields, correlations, discountCurve, rateForecastCurves);            
            List<Date> dates = new List<Date> {anchorDate, anchorDate.AddMonths(6), anchorDate.AddMonths(12) };
            Dividend divi = new Dividend(shares[0]);
            sim.Reset();
            sim.SetRequiredDates(divi, dates);
            sim.SetRequiredDates(shares[0], dates);
            sim.Prepare();
            sim.RunSimulation(0);
            double[] divs = sim.GetIndices(divi, dates);
            double[] shareprices = sim.GetIndices(shares[0], dates);
            double[] fwdRates = sim.GetIndices(FloatingIndex.JIBAR3M, dates); 
            Assert.AreEqual(shareprices[1] * 184.0 / 365 * 0.03, divs[2], 0.01);
            Assert.AreEqual(rateForecastCurves[0].GetForwardRate(dates[1]), fwdRates[1], 0.0001);
        }


        [TestMethod]
        public void TestEquitySimulatorForwardAndVol()
        {
            int N = 100000;
            double[,] sharePrices = Matrix.Zeros(N, 3);
            EquitySimulator sim = new EquitySimulator(shares, prices, vols, divYields, correlations, discountCurve, rateForecastCurves);
            sim.Reset();
            List<Date> simDate = new List<Date> { anchorDate.AddMonths(120) };
            double dt = (simDate[0] - anchorDate) / 365;
            sim.SetRequiredDates(shares[0], simDate);
            sim.Prepare();
            for (int i = 0; i < N; i++)
            {
                sim.RunSimulation(i);
                sharePrices[i,0] = sim.GetIndices(shares[0], simDate)[0];
                sharePrices[i,1] = sim.GetIndices(shares[1], simDate)[0];
                sharePrices[i,2] = sim.GetIndices(shares[2], simDate)[0];
            }
            double mean = sharePrices.GetColumn(0).Mean();
            double refValue = prices[0] * Math.Exp(-divYields[0]* dt) / discountCurve.GetDF(simDate[0]);
            Assert.AreEqual(refValue, mean, 2.0);

            double[,] corr = sharePrices.Log().Correlation();

            Assert.AreEqual(corr[1, 0], 0.4, 0.05);
            Assert.AreEqual(corr[2, 0], 0.5, 0.05);
            Assert.AreEqual(corr[2, 1], 0.6, 0.05);
        }




        [TestMethod]
        public void TestEquitySimulatorProductWithRateForecast()
        {
            Product product = new ProductWithDiviAndFwd();
            // The model
            EquitySimulator sim = new EquitySimulator(shares, prices, vols, divYields, correlations, discountCurve, rateForecastCurves);
            
            Coordinator coordinator = new Coordinator(sim, new List<Simulator> { }, 10000);
            double value = coordinator.Value(new Product[] { product }, anchorDate);
            Assert.AreEqual(31.6, value, 1.0);            
        }

        [TestMethod]
        public void TestEquitySimulatorMultiAssetCall()
        {
            // The model            
            EquitySimulator sim = new EquitySimulator(shares, prices, vols, divYields, correlations, discountCurve, new IFloatingRateSource[0]);
            Coordinator coordinator = new Coordinator(sim, new List<Simulator> { }, 10000);

            // Products
            Date exerciseDate = new Date(2017, 08, 28);
            int p;
            double strike;

            // ALSI
            p = 0;
            strike = prices[p] * 1.05;
            Product call0 = new EuropeanOption(shares[p], strike, exerciseDate);
            double value0 = coordinator.Value(new Product[] { call0 }, anchorDate);
            double refValue0 = BlackEtc.BlackScholes(PutOrCall.Call, strike, (exerciseDate - anchorDate) / 365.0, prices[p],
                                                    vols[p], 0.07, divYields[p]);
            Assert.AreEqual(refValue0, value0, refValue0 * 0.05);

            // AAA
            p = 1;
            strike = prices[p] * 1.05;
            Product call1 = new EuropeanOption(shares[p], strike, exerciseDate);
            double value1 = coordinator.Value(new Product[] { call1 }, anchorDate);
            double refValue1 = BlackEtc.BlackScholes(PutOrCall.Call, strike, (exerciseDate - anchorDate) / 365.0, prices[p],
                                                    vols[p], 0.07, divYields[p]);
            Assert.AreEqual(refValue1, value1, refValue1 * 0.05);

            // BBB
            p = 2;
            strike = prices[p] * 1.05;
            Product call2 = new EuropeanOption(shares[p], strike, exerciseDate);
            double value2 = coordinator.Value(new Product[] { call2 }, anchorDate);
            double refValue2 = BlackEtc.BlackScholes(PutOrCall.Call, strike, (exerciseDate - anchorDate) / 365.0, prices[p],
                                                    vols[p], 0.07, divYields[p]);
            Assert.AreEqual(refValue1, value1, refValue1 * 0.05);

            // All at once
            double valueAll = coordinator.Value(new Product[] { call0, call1, call2 }, anchorDate);
            double refTotal = refValue0 + refValue1 + refValue2;
            Assert.AreEqual(refTotal, valueAll, refTotal * 0.05);
        }
    }
}
