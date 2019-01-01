using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuantSA.Core.MarketData;
using QuantSA.Core.RootFinding;
using QuantSA.CoreExtensions.Curves;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;
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
        }
    }
}