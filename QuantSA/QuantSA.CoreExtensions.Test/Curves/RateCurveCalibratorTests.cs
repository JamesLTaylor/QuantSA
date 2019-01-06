using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.MarketData;
using QuantSA.Core.RootFinding;
using QuantSA.CoreExtensions.Curves;
using QuantSA.CoreExtensions.Curves.Instruments;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.State;
using QuantSA.Solution.Test;

namespace QuantSA.CoreExtensions.Tests.Curves
{
    [TestClass]
    public class RateCurveCalibratorTests
    {
        private readonly Date _calibrationDate = new Date("2018-12-31");
        private readonly Currency _zar = TestHelpers.ZAR;
        private readonly FloatRateIndex _jibar3M = TestHelpers.Jibar3M;
        private readonly FloatRateIndex _jibar1D = TestHelpers.Jibar1D;

        [TestInitialize]
        public void TestInitialize()
        {
            QuantSAState.SetLogger(new TestLogger());
        }

        [TestMethod]
        public void RateCurveCalibrator_CanCalibrateSingleCurve()
        {
            var instruments = new List<IRateCurveInstrument>();
            DiscountingSourceDescription discountCurve = new DiscountingSourceDescription(TestHelpers.ZAR);
            instruments.Add(new DepoCurveInstrument(Tenor.FromMonths(1), 0.071, discountCurve));
            instruments.Add(new DepoCurveInstrument(Tenor.FromMonths(3), 0.072, discountCurve));
            instruments.Add(new DepoCurveInstrument(Tenor.FromMonths(6), 0.073, discountCurve));
            instruments.Add(new FRACurveInstrument(Tenor.FromMonths(6), Tenor.FromMonths(9), TestHelpers.Jibar3M, 0.073));

            var calib = new RateCurveCalibrator(instruments, new MultiDimNewton(1e-8, 100), discountCurve,
                new FloatRateIndex[] {TestHelpers.Jibar3M });
            var mdc = new MarketDataContainer();
            mdc.Set(calib);
            calib.TryCalibrate(_calibrationDate, mdc);
            var testValues = instruments.Select(inst => Math.Abs(inst.Objective()));
            var maxTestValue = testValues.Max();
            Assert.AreEqual(0.0, maxTestValue, 1e-8);
        }

        [TestMethod]
        public void RateCurveCalibrator_CanCalibrateTwoCurves()
        {
            var instruments = new List<IRateCurveInstrument>();
            var zarCsaCurveDescription = new DiscountingSourceDescription(_zar, new BankAccountNumeraire(_zar));
            var jibarDiscountDescription = new DiscountingSourceDescription(_zar, _jibar3M);
            instruments.Add(new DepoCurveInstrument(Tenor.FromMonths(3), 0.071, jibarDiscountDescription));
            instruments.Add(new FixedFloatSwapCurveInstrument(Tenor.FromMonths(6), _jibar3M, 0.0, 0.07,
                zarCsaCurveDescription, FixedFloatSwapCurveInstrument.CurveToStrip.Forecast));
            instruments.Add(new FixedFloatSwapCurveInstrument(Tenor.FromYears(2), _jibar3M, 0.0, 0.07,
                zarCsaCurveDescription, FixedFloatSwapCurveInstrument.CurveToStrip.Forecast));

            instruments.Add(new DepoCurveInstrument(Tenor.FromDays(1), 0.06, zarCsaCurveDescription));
            instruments.Add(new BasisSwapCurveInstrument(Tenor.FromYears(1), _jibar3M, _jibar1D, 0.0, 0.01,
                zarCsaCurveDescription, BasisSwapCurveInstrument.CurveToStrip.DiscountCurve));
            instruments.Add(new BasisSwapCurveInstrument(Tenor.FromYears(2), _jibar3M, _jibar1D, 0.0, 0.01,
                zarCsaCurveDescription, BasisSwapCurveInstrument.CurveToStrip.DiscountCurve));

            var calib = new RateCurveCalibrator(instruments, new MultiDimNewton(1e-8, 100), 
                zarCsaCurveDescription, new FloatRateIndex[] { _jibar1D }, 
                jibarDiscountDescription, new FloatRateIndex[] {_jibar3M});
            var mdc = new MarketDataContainer();
            mdc.Set(calib);
            calib.TryCalibrate(_calibrationDate, mdc);
            var testValues = instruments.Select(inst => Math.Abs(inst.Objective()));
            var maxTestValue = testValues.Max();
            Assert.AreEqual(0.0, maxTestValue, 1e-8);
        }
    }
}