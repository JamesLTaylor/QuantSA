using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.MarketData;
using QuantSA.Core.Optimization;
using QuantSA.Core.RootFinding;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.CoreExtensions.Curves
{
    internal class InitialValues
    {
        private readonly Dictionary<string, List<Tuple<string, Date, double>>> _storage =
            new Dictionary<string, List<Tuple<string, Date, double>>>();

        internal void Add(Tuple<string, Date, double> value)
        {
            if (!_storage.TryGetValue(value.Item1, out var list))
            {
                list = new List<Tuple<string, Date, double>>();
                _storage[value.Item1] = list;
            }

            list.Add(value);
        }

        internal (Date[] dates, double[] values) GetValues(IEnumerable<string> curveNames)
        {
            var combined = new List<Tuple<string, Date, double>>();
            foreach (var curveName in curveNames)
                combined.AddRange(_storage[curveName]);
            combined.Sort(Comparison);
            var dates = combined.Select(t => t.Item2).ToArray();
            var values = combined.Select(t => t.Item3).ToArray();
            return (dates, values);
        }

        private int Comparison(Tuple<string, Date, double> x, Tuple<string, Date, double> y)
        {
            return x.Item2.CompareTo(y.Item2);
        }
    }


    internal class ObjectiveFunction : IObjectiveVectorFunction
    {
        private readonly List<Func<double>> _benchmarkObjectives;
        private readonly DatesAndRates _curve;

        public ObjectiveFunction(DatesAndRates curve, IEnumerable<Func<double>> benchmarkObjectives)
        {
            _curve = curve;
            _benchmarkObjectives = benchmarkObjectives.ToList();
            Value = Vector<double>.Build.Dense(_benchmarkObjectives.Count);
        }

        public Vector<double> Point { get; set; }

        public Vector<double> Value { get; set; }

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;
            _curve.Mutate(point.AsArray());
            for (var i = 0; i < _benchmarkObjectives.Count; i++) Value[i] = _benchmarkObjectives[i]();
        }
    }

    public class RateCurveCalibrator : IMarketDataSource
    {
        private readonly DiscountingSourceDescription _curveToStrip;

        private readonly Dictionary<string, IFloatingRateSource> _floatingRateSources =
            new Dictionary<string, IFloatingRateSource>();

        private readonly IEnumerable<FloatRateIndex> _indicesToBaseOffDiscountCurve;
        private readonly List<IRateCurveInstrument> _instruments;
        private readonly IVectorRootFinder _rootFinder;
        private Date _calibrationDate;
        private DatesAndRates _curve;

        public RateCurveCalibrator(List<IRateCurveInstrument> instruments, IVectorRootFinder rootFinder,
            DiscountingSourceDescription curveToStrip, IEnumerable<FloatRateIndex> indicesToBaseOffDiscountCurve)
        {
            _instruments = instruments;
            _rootFinder = rootFinder;
            _curveToStrip = curveToStrip;
            _indicesToBaseOffDiscountCurve = indicesToBaseOffDiscountCurve ??
                                             throw new ArgumentNullException(nameof(indicesToBaseOffDiscountCurve));
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
            if (_floatingRateSources.ContainsKey(marketDataDescription.Name)) return true;
            return false;
        }

        public T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource
        {
            if (marketDataDescription.Name == _curveToStrip.Name) return _curve as T;
            if (_floatingRateSources.TryGetValue(marketDataDescription.Name, out var floatingRateSource))
                return floatingRateSource as T;
            return null;
        }

        public bool TryCalibrate(Date calibrationDate, IMarketDataContainer marketDataContainer)
        {
            _calibrationDate = calibrationDate;
            var initialValues = new InitialValues();
            foreach (var instrument in _instruments)
            {
                instrument.SetCalibrationDate(calibrationDate);
                initialValues.Add(instrument.GetInitialValue());
            }

            // first curve collection
            var curveNames = new List<string> {_curveToStrip.Name};
            curveNames.AddRange(
                _indicesToBaseOffDiscountCurve.Select(ind => new FloatingRateSourceDescription(ind).Name));

            var initial = initialValues.GetValues(curveNames);

            _curve = new DatesAndRates(_curveToStrip.Currency, calibrationDate,
                initial.dates, initial.values);
            foreach (var index in _indicesToBaseOffDiscountCurve)
            {
                var name = new FloatingRateSourceDescription(index).Name;
                _floatingRateSources[name] = new ForecastCurveFromDiscount(_curve, index, null);
            }

            foreach (var instrument in _instruments) instrument.SetMarketData(marketDataContainer);

            var objective = new ObjectiveFunction(_curve, _instruments.Select(i => (Func<double>) i.Objective));
            var result = _rootFinder.FindRoot(objective, new DenseVector(initial.values));
            return true;
        }
    }
}