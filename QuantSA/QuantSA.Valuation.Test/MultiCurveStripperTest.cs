using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.Products.Rates;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Valuation;

namespace ValuationTest
{
    [TestClass]
    public class MultiCurveStripperTest
    {
        [TestMethod]
        public void TestCurveStripSingleCurve()
        {
            var valueDate = new Date(2017, 1, 13);
            var zar = Currency.ZAR;
            var N = 1.0;
            var r1 = 0.07;
            var r2 = 0.075;
            var date1 = valueDate.AddMonths(3);
            var date2 = valueDate.AddMonths(6);

            var zarDiscUSDColl = new ZeroRatesCurveForStripping(valueDate, zar);
            var zarDisc = new ZeroRatesCurveForStripping(valueDate, zar);

            Product depo1 = new CashLeg(new[] {valueDate, date1}, new[] {-N, N * (1 + r1 * 0.25)}, new[] {zar, zar});
            Product depo2 = new CashLeg(new[] {valueDate, date2}, new[] {-N, N * (1 + r2 * 0.5)}, new[] {zar, zar});

            var modelZARDisc = new DeterminsiticCurves(zarDisc);
            var coordZARDisc = new Coordinator(modelZARDisc, new List<Simulator>(), 1);
            coordZARDisc.SetThreadedness(false);

            var mcs = new MultiCurveStripper(valueDate);
            mcs.AddDiscounting(depo1, () => coordZARDisc.Value(depo1, valueDate), N, 1.0, zarDisc);
            mcs.AddDiscounting(depo2, () => coordZARDisc.Value(depo2, valueDate), N, 1.0, zarDisc);
            mcs.Strip();

            Assert.AreEqual(N, coordZARDisc.Value(depo1, valueDate), 1e-6);
            Assert.AreEqual(N, coordZARDisc.Value(depo2, valueDate), 1e-6);
        }

        /// <summary>
        /// Strip a discounting curve from depos (linear in continuous zero rate) and a forward 
        /// rate forcast curve from a swap (linear in forward rates) using the approximate shape of the 
        /// underlying discount curve.
        /// </summary>
        [TestMethod]
        public void TestCurveStripSeparateForecastAndDiscount()
        {
            var valueDate = new Date(2017, 1, 13);
            var zar = Currency.ZAR;
            var N = 1.0;
            var r1 = 0.12;
            var r2 = 0.08;
            var date1 = valueDate.AddMonths(6);
            var date2 = valueDate.AddMonths(12);

            var zarDisc = new ZeroRatesCurveForStripping(valueDate, zar);
            var jibar3mForecast = new ForwardRatesCurveForStripping(valueDate, FloatRateIndex.JIBAR3M, zarDisc);

            Product depo1 = new CashLeg(new[] {valueDate, date1}, new[] {-N, N * (1 + r1 * 0.5)}, new[] {zar, zar});
            Product depo2 = new CashLeg(new[] {valueDate, date2}, new[] {-N, N * (1 + r2 * 1)}, new[] {zar, zar});
            Product swap = IRSwap.CreateZARSwap(0.08, true, 1.0, valueDate, Tenor.FromMonths(9));

            var modelZARDisc = new DeterminsiticCurves(zarDisc);
            modelZARDisc.AddRateForecast(jibar3mForecast);
            var coordZARDisc = new Coordinator(modelZARDisc, new List<Simulator>(), 1);
            coordZARDisc.SetThreadedness(false);

            var mcs = new MultiCurveStripper(valueDate);
            mcs.AddDiscounting(depo1, () => coordZARDisc.Value(depo1, valueDate), N, 1.0, zarDisc);
            mcs.AddDiscounting(depo2, () => coordZARDisc.Value(depo2, valueDate), N, 1.0, zarDisc);
            mcs.AddForecast(swap, () => coordZARDisc.Value(swap, valueDate), 0.0, 1.0, jibar3mForecast,
                FloatRateIndex.JIBAR3M);
            mcs.Strip();

            Assert.AreEqual(N, coordZARDisc.Value(depo1, valueDate), 1e-6);
            Assert.AreEqual(N, coordZARDisc.Value(depo2, valueDate), 1e-6);
            Assert.AreEqual(0, coordZARDisc.Value(swap, valueDate), 1e-6);

            /*double[] fwdRates = new double[180];
            for (int i=0; i<180; i++ )
            {
                fwdRates[i] = jibar3mForecast.GetForwardRate(valueDate.AddTenor(Tenor.Days(i)));
            }
            Debug.WriteToFile("c:\\dev\\quantsa\\temp\\fwdRates.csv", fwdRates);
            */
        }

        /// <summary>
        /// Tests the curve strip with a specified underlying DF shape.  In particular a 5% turn on a single date.
        /// </summary>
        [TestMethod]
        public void TestCurveStripWithUnderlying()
        {
            var valueDate = new Date(2017, 1, 13);
            var zar = Currency.ZAR;
            var N = 1.0;
            var r1 = 0.07;
            var r2 = 0.075;
            var date1 = valueDate.AddMonths(3);
            var date2 = valueDate.AddMonths(6);
            var turnSize = 0.05;
            IDiscountingSource turnShapeCurve = new DFCurveWithTurn(valueDate, zar, new Date(2017, 2, 20), turnSize);
            var zarDisc = new ZeroRatesCurveForStripping(valueDate, turnShapeCurve);

            Product depo1 = new CashLeg(new[] {valueDate, date1}, new[] {-N, N * (1 + r1 * 0.25)}, new[] {zar, zar});
            Product depo2 = new CashLeg(new[] {valueDate, date2}, new[] {-N, N * (1 + r2 * 0.5)}, new[] {zar, zar});

            var modelZARDisc = new DeterminsiticCurves(zarDisc);
            var coordZARDisc = new Coordinator(modelZARDisc, new List<Simulator>(), 1);
            coordZARDisc.SetThreadedness(false);

            var mcs = new MultiCurveStripper(valueDate);
            mcs.AddDiscounting(depo1, () => coordZARDisc.Value(depo1, valueDate), N, 1.0, zarDisc);
            mcs.AddDiscounting(depo2, () => coordZARDisc.Value(depo2, valueDate), N, 1.0, zarDisc);
            mcs.Strip();

            Assert.AreEqual(N, coordZARDisc.Value(depo1, valueDate), 1e-6);
            Assert.AreEqual(N, coordZARDisc.Value(depo2, valueDate), 1e-6);
            var df1 = zarDisc.GetDF(new Date(2017, 2, 19));
            var df2 = zarDisc.GetDF(new Date(2017, 2, 20));
            var df3 = zarDisc.GetDF(new Date(2017, 2, 21));
            var df4 = zarDisc.GetDF(new Date(2017, 2, 22));
            var rate1 = 365.0 * (df1 / df2 - 1);
            var rate2 = 365.0 * (df2 / df3 - 1);
            var rate3 = 365.0 * (df3 / df4 - 1);
            Assert.AreEqual(turnSize, rate2 - rate1, turnSize / 100);
            Assert.AreEqual(turnSize, rate2 - rate3, turnSize / 100);
        }

        /// <summary>
        /// Helper function to make a depo
        /// </summary>
        private Product MakeDepo(Date anchorDate, double rate, int months)
        {
            return new CashLeg(new[] {anchorDate, anchorDate.AddMonths(months)},
                new[] {-1.0, 1.0 * (1 + rate * months / 12.0)},
                new[] {Currency.ZAR, Currency.ZAR});
        }

        /// <summary>
        /// Here we assume that we know the USD collateral implied ZAR discount curve, we imply 
        /// Jibar forwards from the USD collateralized swaps and then imply a new discount curve 
        /// from uncollateralized swap quotes.  The key here is that the same bootstrapping call
        /// gets swaps with collateral discounting and swaps with staight zar discounting.
        /// </summary>
        [TestMethod]
        public void TestCurveStripTwoZARDiscount()
        {
            var valueDate = new Date(2017, 1, 13);
            var zar = Currency.ZAR;

            // Empty curves
            var zarDiscUSDColl = new ZeroRatesCurveForStripping(valueDate, zar);
            var zarDisc = new ZeroRatesCurveForStripping(valueDate, zar);
            var jibarCurve = new ForwardRatesCurveForStripping(valueDate, FloatRateIndex.JIBAR3M);

            // Models
            var modelZARDiscUSDColl = new DeterminsiticCurves(zarDiscUSDColl);
            modelZARDiscUSDColl.AddRateForecast(jibarCurve);
            var coordZARDiscUSDColl = new Coordinator(modelZARDiscUSDColl, new List<Simulator>(), 1);
            coordZARDiscUSDColl.SetThreadedness(false);
            var modelZARDisc = new DeterminsiticCurves(zarDisc);
            modelZARDisc.AddRateForecast(jibarCurve); // same jibar curve in both coordinators.
            var coordZARDisc = new Coordinator(modelZARDisc, new List<Simulator>(), 1);
            coordZARDisc.SetThreadedness(false);

            //Instruments for USDColl discounting curve.
            var mcs = new MultiCurveStripper(valueDate);
            var depo1 = MakeDepo(valueDate, 0.07, 24);
            var depo2 = MakeDepo(valueDate, 0.072, 48);
            mcs.AddDiscounting(depo1, () => coordZARDiscUSDColl.Value(depo1, valueDate), 1.0, 1.0, zarDiscUSDColl);
            mcs.AddDiscounting(depo2, () => coordZARDiscUSDColl.Value(depo2, valueDate), 1.0, 1.0, zarDiscUSDColl);

            Product swapUSDColl1 = IRSwap.CreateZARSwap(0.07, true, 1.0, valueDate, Tenor.FromMonths(24));
            Product swapUSDColl2 = IRSwap.CreateZARSwap(0.072, true, 1.0, valueDate, Tenor.FromMonths(48));
            mcs.AddForecast(swapUSDColl1, () => coordZARDiscUSDColl.Value(swapUSDColl1, valueDate), 0, 1, jibarCurve,
                FloatRateIndex.JIBAR3M);
            mcs.AddForecast(swapUSDColl2, () => coordZARDiscUSDColl.Value(swapUSDColl2, valueDate), 0, 1, jibarCurve,
                FloatRateIndex.JIBAR3M);

            Product swapNoColl1 = IRSwap.CreateZARSwap(0.0709, true, 1.0, valueDate, Tenor.FromMonths(36));
            Product swapNoColl2 = IRSwap.CreateZARSwap(0.0719, true, 1.0, valueDate, Tenor.FromMonths(48));
            mcs.AddDiscounting(swapNoColl1, () => coordZARDisc.Value(swapNoColl1, valueDate), 0, 1, zarDisc);
            mcs.AddDiscounting(swapNoColl2, () => coordZARDisc.Value(swapNoColl2, valueDate), 0, 1, zarDisc);

            mcs.Strip();

            Assert.AreEqual(1.0, coordZARDiscUSDColl.Value(depo1, valueDate), 1e-6);
            Assert.AreEqual(1.0, coordZARDiscUSDColl.Value(depo2, valueDate), 1e-6);
            Assert.AreEqual(0.0, coordZARDiscUSDColl.Value(swapUSDColl2, valueDate), 1e-6);
            Assert.AreEqual(0.0, coordZARDiscUSDColl.Value(swapUSDColl2, valueDate), 1e-6);
            //Assert.AreNotEqual(0.0, coordZARDisc.Value(swapUSDColl1, valueDate), 1e-6); No discount sensitivity
            Assert.AreNotEqual(0.0, coordZARDisc.Value(swapUSDColl2, valueDate), 1e-6);
            Assert.AreEqual(0.0, coordZARDisc.Value(swapNoColl1, valueDate), 1e-6);
            Assert.AreEqual(0.0, coordZARDisc.Value(swapNoColl2, valueDate), 1e-6);
        }

        [TestMethod]
        public void TestCurveStripSameForecastAndDiscount()
        {
        }

        /// <summary>
        /// A curve that ensures that the overnight rate as calculated on the turnDate will be 
        /// different from the rate before or after that by turnSize.
        /// </summary>
        /// <seealso cref="IDiscountingSource" />
        private class DFCurveWithTurn : IDiscountingSource
        {
            private readonly Date anchorDate;
            private readonly Currency ccy;
            private readonly double postTurnDF;
            private readonly Date turnDate;

            /// <summary>
            /// Initializes a new instance of the <see cref="DFCurveWithTurn"/> class.
            /// </summary>
            /// <param name="anchorDate">The anchor date.</param>
            /// <param name="ccy">The ccy.</param>
            /// <param name="turnDate">The turn date.</param>
            /// <param name="turnSize">As an absolute number in rate.  eg. 25bp turn is entered as 0.0025.</param>
            public DFCurveWithTurn(Date anchorDate, Currency ccy, Date turnDate, double turnSize)
            {
                this.anchorDate = anchorDate;
                this.ccy = ccy;
                this.turnDate = turnDate;
                postTurnDF = Math.Exp(-turnSize / 365.0);
            }

            public Date GetAnchorDate()
            {
                return anchorDate;
            }

            public Currency GetCurrency()
            {
                return ccy;
            }

            public double GetDF(Date date)
            {
                return date > turnDate ? postTurnDF : 1.0;
            }
        }
    }
}