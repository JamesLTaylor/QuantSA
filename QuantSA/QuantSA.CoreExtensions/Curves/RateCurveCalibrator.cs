using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Optimization;
using QuantSA.Core.RootFinding;
using QuantSA.General.Conventions.DayCount;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.Primitives;

namespace QuantSA.CoreExtensions.Curves
{
    public interface IRateCurveInstrument
    {
        void SetCalibrationDate(Date calibrationDate);

        Tuple<Date, double> GetInitialValue();

        double Objective();
        void SetMarketData(IMarketDataContainer marketData);
    }

    public class DepoCurveInstrument : IRateCurveInstrument
    {
        private readonly Currency _currency;
        private readonly double _simpleRate;
        private readonly Tenor _tenor;
        private Date _calibrationDate;
        private double _cf;
        private IDiscountingSource _curve;
        private Date _maturityDate;

        public DepoCurveInstrument(Currency currency, Tenor tenor, double simpleRate)
        {
            _currency = currency;
            _tenor = tenor;
            _simpleRate = simpleRate;
        }

        public void SetCalibrationDate(Date calibrationDate)
        {
            _calibrationDate = calibrationDate;
            _maturityDate = calibrationDate.AddTenor(_tenor);
            var yf = Actual365Fixed.Instance.YearFraction(_calibrationDate, _maturityDate);
            _cf = 1e6 * (1 + _simpleRate * yf);
        }

        public Tuple<Date, double> GetInitialValue()
        {
            return new Tuple<Date, double>(_maturityDate, _simpleRate);
        }

        public void SetMarketData(IMarketDataContainer marketData)
        {
            _curve = marketData.Get(new DiscountingSourceDescription(_currency));
        }

        public double Objective()
        {
            var df = _curve.GetDF(_maturityDate);
            return df * _cf - 1e6;
        }
    }

    /// <summary>
    /// Specifies what the curve that should be stripped will look like.
    /// </summary>
    public interface IRateCurveSpecification
    {
    }

    public class SingleCurveSpecification
    {
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
        private readonly Currency _currency;
        private readonly List<IRateCurveInstrument> _instruments;
        private readonly IVectorRootFinder _rootFinder;
        private DatesAndRates _curve;
        private Date _calibrationDate;

        public RateCurveCalibrator(List<IRateCurveInstrument> instruments, IVectorRootFinder rootFinder,
            Currency currency)
        {
            _instruments = instruments;
            _rootFinder = rootFinder;
            _currency = currency;
        }

        public Date GetAnchorDate()
        {
            return _calibrationDate;
        }

        public string GetName()
        {
            return nameof(RateCurveCalibrator);
        }

        public bool CanBeA<T>(MarketDataDescription<T> description, IMarketDataContainer marketDataContainer)
            where T : class, IMarketDataSource
        {
            return true;
        }

        public T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource
        {
            return _curve as T;
        }

        public bool TryCalibrate(Date calibrationDate, IMarketDataContainer marketDataContainer)
        {
            _calibrationDate = calibrationDate;
            var initialValues = new List<Tuple<Date, double>>();
            foreach (var instrument in _instruments)
            {
                instrument.SetCalibrationDate(calibrationDate);
                initialValues.Add(instrument.GetInitialValue());
            }

            initialValues.Sort(Comparison);
            var initialGuess = new DenseVector(initialValues.Select(t => t.Item2).ToArray());
            _curve = new DatesAndRates(_currency, calibrationDate, initialValues.Select(t => t.Item1).ToArray(),
                initialGuess.AsArray());
            foreach (var instrument in _instruments)
            {
                instrument.SetMarketData(marketDataContainer);
            }

            var objective = new ObjectiveFunction(_curve, _instruments.Select(i => (Func<double>) i.Objective));
            var result = _rootFinder.FindRoot(objective, initialGuess);
            return true;
        }

        private int Comparison(Tuple<Date, double> x, Tuple<Date, double> y)
        {
            return x.Item1.CompareTo(y.Item1);
        }
    }
}