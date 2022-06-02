using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Shared.Dates;
using QuantSA.Solution.Test;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.MarketData;
using QuantSA.CoreExtensions.Products.Rates;
using QuantSA.Core.MarketData;
using System;


namespace QuantSA.CoreExtensions.Test.SAMarket
{
    [TestClass]

    public class InflationLinkedSwapTest
    {
        [TestMethod]
        public void TestInflationLinkedSwap()
        {
            //Testing the zero-coupon inflation swap
            //Parameterise test inputs below
            var payFixed = -1;
            var startDate = new Date(2005, 8, 31);
            var nominal = (double) 5000000;
            var tenor = Tenor.FromYears(2);
            var fixedRate = 0.036;
            var ccy = TestHelpers.ZAR;
            var index = new FloatRateIndex("ZAR.JIBAR.3M", ccy, "JIBAR", Tenor.FromMonths(3));
            var spread = 0.0;

            Date[] cpiDates =
            { new Date(2005,3,1), new Date(2005,4,1), new Date(2005,5,1), new Date(2005,6,1), new Date(2005,7,1), new Date(2005,8,1),
              new Date(2005,9,1), new Date(2005,10,1), new Date(2005,11,1), new Date(2005,12,1), new Date(2006,1,1), new Date(2006,2,1),
              new Date(2006,3,1), new Date(2006,4,1), new Date(2006,5,1), new Date(2006,6,1), new Date(2006,7,1), new Date(2006,8,1),
              new Date(2006,9,1), new Date(2006,10,1), new Date(2006,11,1), new Date(2006,12,1), new Date(2007,1,1), new Date(2007,2,1),
              new Date(2007,3,1), new Date(2007,4,1), new Date(2007,5,1), new Date(2007,6,1), new Date(2007,7,1), new Date(2007,8,1),
              new Date(2007,9,1),new Date(2007,10,1)
,             };

            double[] cpiRates=
            {
                126.90, 127.60, 127.60, 127.40, 128.50, 129.0, 129.50, 129.60, 129.50, 129.50, 130.40, 130.50, 131.20, 131.80, 132.60, 133.60, 134.90,
                136.0, 136.30, 136.60, 136.50, 137.0, 138.20, 139.20, 138.0, 141.0, 141.80, 143.0, 144.40, 145.10, 146.10, 147.40
            };

            var zaCalendar = new Calendar("Test");

            Date[] curveDates =
{
                new Date(2005, 8, 31), new Date(2005, 11, 30), new Date(2006, 2, 28), new Date(2006, 5, 31), new Date(2006, 8, 31), new Date(2006, 11, 30),
                new Date(2007, 2, 28), new Date(2007, 5, 31) };

            double[] curveRates = { 0.07004, 0.07164, 0.07092, 0.07079, 0.08224, 0.08918, 0.09075, 0.09354 };

            //Create curve used to determine swap cash flows
            IFloatingRateSource forecastCurve = new ForecastCurve(startDate, index, curveDates, curveRates);

            //Create instance of an inflation swap
            var inflationSwap = InflationLinkedSwapEx.CreateInflationLinkedSwap(payFixed, startDate, nominal, tenor, fixedRate, index, spread, zaCalendar, ccy);
            
            //Get results
            var results = inflationSwap.InflationLinkedSwapMeasures(cpiDates, cpiRates, forecastCurve);

            Assert.AreEqual(5856959.45, Math.Round((double)results.GetScalar(InflationLinkedSwapEx.Keys.FloatingLegCashFlows), 2), 1e-8);
            Assert.AreEqual(-5966334.90, Math.Round((double)results.GetScalar(InflationLinkedSwapEx.Keys.FixedLegCashFlows), 2), 1e-8);
            Assert.AreEqual(-109375.45, Math.Round((double)results.GetScalar(InflationLinkedSwapEx.Keys.NetCashFlows), 2), 1e-8);
        }
    }
}

