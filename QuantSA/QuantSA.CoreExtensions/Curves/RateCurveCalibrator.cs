using System;
using System.Collections.Generic;
using System.Linq;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.MarketData;
using QuantSA.Core.RootFinding;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.CoreExtensions.Curves
{
    /// <summary>
    /// Calibrate up to two discounting curves with an number of forecast curves based off them.
    /// </summary>
    public class RateCurveCalibrator : IMarketDataSource
    {
        private readonly DiscountingSourceDescription _curveToStrip;

        private readonly Dictionary<string, IFloatingRateSource> _floatingRateSources =
            new Dictionary<string, IFloatingRateSource>();

        private readonly IEnumerable<FloatRateIndex> _indicesToBaseOffDiscountCurve;
        private readonly IEnumerable<FloatRateIndex> _indicesToBaseOffSecondCurve;
        private readonly List<IRateCurveInstrument> _instruments;
        private readonly IVectorRootFinder _rootFinder;
        private readonly DiscountingSourceDescription _secondCurveToStrip;
        private Date _calibrationDate;
        private DatesAndRates _curve;
        private DatesAndRates _secondCurve;

        /// <summary>
        /// Create a calibrator for a single discounting curve and forecast curves based off it for each index in <paramref name="indicesToBaseOffDiscountCurve"/>.
        /// </summary>
        /// <param name="instruments"></param>
        /// <param name="rootFinder"></param>
        /// <param name="curveToStrip"></param>
        /// <param name="indicesToBaseOffDiscountCurve"></param>
        public RateCurveCalibrator(List<IRateCurveInstrument> instruments, IVectorRootFinder rootFinder,
            DiscountingSourceDescription curveToStrip, IEnumerable<FloatRateIndex> indicesToBaseOffDiscountCurve)
        {
            _instruments = instruments;
            _rootFinder = rootFinder;
            _curveToStrip = curveToStrip;
            _indicesToBaseOffDiscountCurve = indicesToBaseOffDiscountCurve ??
                                             throw new ArgumentNullException(nameof(indicesToBaseOffDiscountCurve));
        }

        /// <summary>
        /// Create a calibrator for a two curves and forecast curves based off each of them.
        /// </summary>
        /// <param name="instruments"></param>
        /// <param name="rootFinder"></param>
        /// <param name="curveToStrip"></param>
        /// <param name="indicesToBaseOffDiscountCurve"></param>
        /// <param name="secondCurveToStrip"></param>
        /// <param name="indicesToBaseOffSecondCurve"></param>
        public RateCurveCalibrator(List<IRateCurveInstrument> instruments, IVectorRootFinder rootFinder,
            DiscountingSourceDescription curveToStrip, IEnumerable<FloatRateIndex> indicesToBaseOffDiscountCurve,
            DiscountingSourceDescription secondCurveToStrip, IEnumerable<FloatRateIndex> indicesToBaseOffSecondCurve)
            : this(instruments, rootFinder, curveToStrip, indicesToBaseOffDiscountCurve)
        {
            _secondCurveToStrip = secondCurveToStrip;
            _indicesToBaseOffSecondCurve = indicesToBaseOffSecondCurve ??
                                           throw new ArgumentNullException(nameof(indicesToBaseOffSecondCurve));
        }


        public Date GetAnchorDate()
        {
            return _calibrationDate;
        }

        public string GetName()
        {
            return nameof(RateCurveCalibrator);
        }

        public bool CanBeA<T>(MarketDataDescription<T> marketDataDescription, IMarketDataContainer marketDataContainer)
            where T : class, IMarketDataSource
        {
            if (marketDataDescription.Name == _curveToStrip.Name) return true;
            if (_secondCurveToStrip != null && marketDataDescription.Name == _secondCurveToStrip.Name) return true;
            if (_floatingRateSources.ContainsKey(marketDataDescription.Name)) return true;
            return false;
        }

        public T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource
        {
            if (marketDataDescription.Name == _curveToStrip.Name) return _curve as T;
            if (_secondCurveToStrip != null && marketDataDescription.Name == _secondCurveToStrip.Name)
                return _secondCurve as T;
            if (_floatingRateSources.TryGetValue(marketDataDescription.Name, out var floatingRateSource))
                return floatingRateSource as T;
            return null;
        }

        public bool TryCalibrate(Date calibrationDate, IMarketDataContainer marketDataContainer)
        {
            _calibrationDate = calibrationDate;
            var initialValues = new InitialValueCollector(_calibrationDate, _instruments);

            var objective = new ObjectiveFunction();
            _curve = InitializeCurve(calibrationDate, initialValues, objective, _curveToStrip,
                _indicesToBaseOffDiscountCurve);
            _secondCurve = InitializeCurve(calibrationDate, initialValues, objective, _secondCurveToStrip,
                _indicesToBaseOffSecondCurve);
            objective.SetBenchmarkObjectives(_instruments, marketDataContainer);

            var result = _rootFinder.FindRoot(objective, objective.InitialValues);
            return true;
        }

        private DatesAndRates InitializeCurve(Date calibrationDate, InitialValueCollector initialValueCollector,
            ObjectiveFunction objective, DiscountingSourceDescription curveToStrip,
            IEnumerable<FloatRateIndex> indicesToBaseOffCurve)
        {
            if (curveToStrip == null) return null;
            var curveNames = new List<string> {curveToStrip.Name};
            var indices = indicesToBaseOffCurve.ToList();
            curveNames.AddRange(
                indices.Select(ind => new FloatingRateSourceDescription(ind).Name));
            var initial = initialValueCollector.GetValues(curveNames);
            var curve = new DatesAndRates(curveToStrip.Currency, calibrationDate,
                initial.dates, initial.values);
            foreach (var index in indices)
            {
                var name = new FloatingRateSourceDescription(index).Name;
                _floatingRateSources[name] = new ForecastCurveFromDiscount(curve, index, null);
            }

            objective.AddCurve(curve, initial.values);
            return curve;
        }
    }
}