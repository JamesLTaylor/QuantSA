using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.MarketData;
using QuantSA.Core.RootFinding;
using QuantSA.CoreExtensions.Curves;
using QuantSA.Shared.Dates;
using QuantSA.Solution.Test;

namespace ProductExtensionsTest.Curves
{
    [TestClass]
    public class RateCurveCalibratorTests
    {
        private readonly Date _calibrationDate = new Date("2018-12-31");

        [TestMethod]
        public void RateCurveCalibrator_CanCalibrateSingleCurve()
        {
            var instruments = new List<IRateCurveInstrument>();
            instruments.Add(new DepoCurveInstrument(TestHelpers.ZAR, Tenor.FromMonths(1), 0.071));
            instruments.Add(new DepoCurveInstrument(TestHelpers.ZAR, Tenor.FromMonths(3), 0.072));
            instruments.Add(new DepoCurveInstrument(TestHelpers.ZAR, Tenor.FromMonths(6), 0.073));

            var calib = new RateCurveCalibrator(instruments, new MultiDimNewton(1e-8, 100), TestHelpers.ZAR);
            var mdc = new MarketDataContainer();
            mdc.Set(calib);
            calib.TryCalibrate(_calibrationDate, mdc);
        }
    }
}