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
            {
                if (_storage.TryGetValue(curveName, out var datesAndValues))
                    combined.AddRange(datesAndValues);
            }

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
        private List<Func<double>> _benchmarkObjectives = new List<Func<double>>();
        private DatesAndRates _curve1;
        private DatesAndRates _curve2;
        private double[] _curve1Values;
        private double[] _curve2Values;
        
        public ObjectiveFunction()
        {}

        public void AddCurve(DatesAndRates curve, double[] initialGuess)
        {
            if (_curve1 == null)
            {
                _curve1 = curve;
                _curve1Values = (double[])initialGuess.Clone();
            }
            else if (_curve2 == null)
            {
                _curve2 = curve;
                _curve2Values = (double[])initialGuess.Clone();
            }
            else
                throw new ArgumentException("Only one or two curves can be added.");
        }

        public void SetBenchmarkObjectives(IEnumerable<Func<double>> benchmarkObjectives)
        {
            _benchmarkObjectives = benchmarkObjectives.ToList();
            Value = Vector<double>.Build.Dense(_benchmarkObjectives.Count);
        }

        public Vector<double> Point { get; set; }

        public Vector<double> Value { get; set; }

        public Vector<double> InitialValues
        {
            get
            {
                var list = _curve1Values.ToList();
                if (_curve2!=null) list.AddRange(_curve2Values);
                return new DenseVector(list.ToArray());
            }
        }

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;
            for (var i = 0; i < point.Count; i++)
            {
                if (i < _curve1Values.Length)
                    _curve1Values[i] = point[i];
                else
                    _curve2Values[i- _curve1Values.Length] = point[i];
            }
            
            _curve1.Mutate(_curve1Values);
            _curve2?.Mutate(_curve2Values);

            for (var i = 0; i < _benchmarkObjectives.Count; i++) Value[i] = _benchmarkObjectives[i]();
        }
    }

    public class RateCurveCalibrator : IMarketDataSource
    {
        private readonly DiscountingSourceDescription _curveToStrip;
        private readonly DiscountingSourceDescription _secondCurveToStrip;
        

        private readonly Dictionary<string, IFloatingRateSource> _floatingRateSources =
            new Dictionary<string, IFloatingRateSource>();

        private readonly IEnumerable<FloatRateIndex> _indicesToBaseOffDiscountCurve;
        private readonly IEnumerable<FloatRateIndex> _indicesToBaseOffSecondCurve;
        private readonly List<IRateCurveInstrument> _instruments;
        private readonly IVectorRootFinder _rootFinder;
        private Date _calibrationDate;
        private DatesAndRates _curve;
        private DatesAndRates _secondCurve;

        public RateCurveCalibrator(List<IRateCurveInstrument> instruments, IVectorRootFinder rootFinder,
            DiscountingSourceDescription curveToStrip, IEnumerable<FloatRateIndex> indicesToBaseOffDiscountCurve)
        {
            _instruments = instruments;
            _rootFinder = rootFinder;
            _curveToStrip = curveToStrip;
            _indicesToBaseOffDiscountCurve = indicesToBaseOffDiscountCurve ??
                                             throw new ArgumentNullException(nameof(indicesToBaseOffDiscountCurve));
        }

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
            if (_secondCurveToStrip!=null && marketDataDescription.Name == _secondCurveToStrip.Name) return true;
            if (_floatingRateSources.ContainsKey(marketDataDescription.Name)) return true;
            return false;
        }

        public T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource
        {
            if (marketDataDescription.Name == _curveToStrip.Name) return _curve as T;
            if (_secondCurveToStrip != null && marketDataDescription.Name == _secondCurveToStrip.Name) return _secondCurve as T;
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

            var objective = new ObjectiveFunction();
            // first curve collection
            _curve = InitializeCurve(calibrationDate, initialValues, objective, _curveToStrip, _indicesToBaseOffDiscountCurve);
            if (_secondCurveToStrip!=null ) _secondCurve = InitializeCurve(calibrationDate, initialValues, objective, _secondCurveToStrip, _indicesToBaseOffSecondCurve);

            objective.SetBenchmarkObjectives(_instruments.Select(inst => (Func<double>)inst.Objective));
            foreach (var instrument in _instruments) instrument.SetMarketData(marketDataContainer);
            
            var result = _rootFinder.FindRoot(objective, objective.InitialValues);
            return true;
        }

        private DatesAndRates InitializeCurve(Date calibrationDate, InitialValues initialValues,
            ObjectiveFunction objective, DiscountingSourceDescription curveToStrip, IEnumerable<FloatRateIndex> indicesToBaseOffCurve)
        {
            var curveNames = new List<string> {curveToStrip.Name};
            var indices = indicesToBaseOffCurve.ToList();
            curveNames.AddRange(
                indices.Select(ind => new FloatingRateSourceDescription(ind).Name));
            var initial = initialValues.GetValues(curveNames);
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