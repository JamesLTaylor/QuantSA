using System;
using QuantSA.Valuation;
using QuantSA.General;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ValuationTest
{
    [TestClass]
    public class MultiCurveStripperTest
    {
        [TestMethod]
        public void TestCurveStripSingleCurve()
        {
            Date valueDate = new Date(2017, 1, 13);
            Currency zar = Currency.ZAR;
            double N = 1.0;
            double r1 = 0.07;
            double r2 = 0.075;
            Date date1 = valueDate.AddMonths(3);
            Date date2 = valueDate.AddMonths(6);
            
            ZeroRatesCurveForStripping zarDiscUSDColl = new ZeroRatesCurveForStripping(valueDate, zar);
            ZeroRatesCurveForStripping zarDisc = new ZeroRatesCurveForStripping(valueDate, zar);
            
            Product depo1 = new CashLeg(new Date[] { valueDate, date1 }, new double[] { -N, N * (1 + r1 * 0.25) }, new Currency[] { zar, zar });
            Product depo2 = new CashLeg(new Date[] { valueDate, date2 }, new double[] { -N, N * (1 + r2 * 0.5) }, new Currency[] { zar, zar });

            DeterminsiticCurves modelZARDisc = new DeterminsiticCurves(zarDisc);
            Coordinator coordZARDisc = new Coordinator(modelZARDisc, new List<Simulator>(), 1);
            coordZARDisc.SetThreadedness(false);

            MultiCurveStripper mcs = new MultiCurveStripper(valueDate);
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
            Date valueDate = new Date(2017, 1, 13);
            Currency zar = Currency.ZAR;
            double N = 1.0;
            double r1 = 0.12;
            double r2 = 0.08;
            Date date1 = valueDate.AddMonths(6);
            Date date2 = valueDate.AddMonths(12);

            ZeroRatesCurveForStripping zarDisc = new ZeroRatesCurveForStripping(valueDate, zar);
            ForwardRatesCurveForStripping jibar3mForecast = new ForwardRatesCurveForStripping(valueDate, FloatingIndex.JIBAR3M, zarDisc);

            Product depo1 = new CashLeg(new Date[] { valueDate, date1 }, new double[] { -N, N * (1 + r1 * 0.5) }, new Currency[] { zar, zar });
            Product depo2 = new CashLeg(new Date[] { valueDate, date2 }, new double[] { -N, N * (1 + r2 * 1) }, new Currency[] { zar, zar });
            Product swap = IRSwap.CreateZARSwap(0.08, true, 1.0, valueDate, Tenor.Months(9));

            DeterminsiticCurves modelZARDisc = new DeterminsiticCurves(zarDisc);
            modelZARDisc.AddRateForecast(jibar3mForecast);
            Coordinator coordZARDisc = new Coordinator(modelZARDisc, new List<Simulator>(), 1);
            coordZARDisc.SetThreadedness(false);

            MultiCurveStripper mcs = new MultiCurveStripper(valueDate);
            mcs.AddDiscounting(depo1, () => coordZARDisc.Value(depo1, valueDate), N, 1.0, zarDisc);
            mcs.AddDiscounting(depo2, () => coordZARDisc.Value(depo2, valueDate), N, 1.0, zarDisc);
            mcs.AddForecast(swap, () => coordZARDisc.Value(swap, valueDate), 0.0, 1.0, jibar3mForecast, FloatingIndex.JIBAR3M);
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
        /// A curve that ensures that the overnight rate as calculated on the turnDate will be 
        /// different from the rate before or after that by turnSize.
        /// </summary>
        /// <seealso cref="QuantSA.General.IDiscountingSource" />
        private class DFCurveWithTurn : IDiscountingSource
        {
            private Date anchorDate;
            private Currency ccy;
            private Date turnDate;
            private double postTurnDF;

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
            public Date GetAnchorDate() { return anchorDate; }
            public Currency GetCurrency() { return ccy; }
            public double GetDF(Date date) { return date > turnDate ? postTurnDF : 1.0; }
        }

        /// <summary>
        /// Tests the curve strip with a specified underlying DF shape.  In particular a 5% turn on a single date.
        /// </summary>
        [TestMethod]
        public void TestCurveStripWithUnderlying()
        {
            Date valueDate = new Date(2017, 1, 13);
            Currency zar = Currency.ZAR;
            double N = 1.0;
            double r1 = 0.07;
            double r2 = 0.075;
            Date date1 = valueDate.AddMonths(3);
            Date date2 = valueDate.AddMonths(6);
            double turnSize = 0.05;
            IDiscountingSource turnShapeCurve = new DFCurveWithTurn(valueDate, zar, new Date(2017, 2, 20), turnSize);
            ZeroRatesCurveForStripping zarDisc = new ZeroRatesCurveForStripping(valueDate, turnShapeCurve);

            Product depo1 = new CashLeg(new Date[] { valueDate, date1 }, new double[] { -N, N * (1 + r1 * 0.25) }, new Currency[] { zar, zar });
            Product depo2 = new CashLeg(new Date[] { valueDate, date2 }, new double[] { -N, N * (1 + r2 * 0.5) }, new Currency[] { zar, zar });

            DeterminsiticCurves modelZARDisc = new DeterminsiticCurves(zarDisc);
            Coordinator coordZARDisc = new Coordinator(modelZARDisc, new List<Simulator>(), 1);
            coordZARDisc.SetThreadedness(false);

            MultiCurveStripper mcs = new MultiCurveStripper(valueDate);
            mcs.AddDiscounting(depo1, () => coordZARDisc.Value(depo1, valueDate), N, 1.0, zarDisc);
            mcs.AddDiscounting(depo2, () => coordZARDisc.Value(depo2, valueDate), N, 1.0, zarDisc);
            mcs.Strip();

            Assert.AreEqual(N, coordZARDisc.Value(depo1, valueDate), 1e-6);
            Assert.AreEqual(N, coordZARDisc.Value(depo2, valueDate), 1e-6);
            double df1 = zarDisc.GetDF(new Date(2017, 2, 19));
            double df2 = zarDisc.GetDF(new Date(2017, 2, 20));
            double df3 = zarDisc.GetDF(new Date(2017, 2, 21));
            double df4 = zarDisc.GetDF(new Date(2017, 2, 22));
            double rate1 = 365.0 * (df1 / df2 - 1);
            double rate2 = 365.0 * (df2 / df3 - 1);
            double rate3 = 365.0 * (df3 / df4 - 1);
            Assert.AreEqual(turnSize, rate2 - rate1, turnSize/100);
            Assert.AreEqual(turnSize, rate2 - rate3, turnSize / 100);
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
        }

        [TestMethod]
        public void TestCurveStripSameForecastAndDiscount()
        {
        }

    }
}
