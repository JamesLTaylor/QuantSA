using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Core.Optimization;
using QuantSA.Shared.MarketData;

namespace QuantSA.CoreExtensions.Curves
{
    internal class ObjectiveFunction : IObjectiveVectorFunction
    {
        private List<Func<double>> _benchmarkObjectives = new List<Func<double>>();
        private DatesAndRates _curve1;
        private double[] _curve1Values;
        private DatesAndRates _curve2;
        private double[] _curve2Values;

        public Vector<double> InitialValues
        {
            get
            {
                var list = _curve1Values.ToList();
                if (_curve2 != null) list.AddRange(_curve2Values);
                return new DenseVector(list.ToArray());
            }
        }

        public Vector<double> Point { get; set; }

        public Vector<double> Value { get; set; }

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;
            for (var i = 0; i < point.Count; i++)
                if (i < _curve1Values.Length)
                    _curve1Values[i] = point[i];
                else
                    _curve2Values[i - _curve1Values.Length] = point[i];

            _curve1.Mutate(_curve1Values);
            _curve2?.Mutate(_curve2Values);

            for (var i = 0; i < _benchmarkObjectives.Count; i++)
            {
                Value[i] = _benchmarkObjectives[i]();
                if (double.IsNaN(Value[i]))
                    throw new NonConvergenceException("Instrument can not be valued with current value of curves.");
            }
        }

        public void AddCurve(DatesAndRates curve, double[] initialGuess)
        {
            if (_curve1 == null)
            {
                _curve1 = curve;
                _curve1Values = (double[]) initialGuess.Clone();
            }
            else if (_curve2 == null)
            {
                _curve2 = curve;
                _curve2Values = (double[]) initialGuess.Clone();
            }
            else
            {
                throw new ArgumentException("Only one or two curves can be added.");
            }
        }


        public void SetBenchmarkObjectives(IEnumerable<IRateCurveInstrument> instruments, IMarketDataContainer marketDataContainer)
        {
            foreach (var instrument in instruments)
            {
                instrument.SetMarketData(marketDataContainer);
                _benchmarkObjectives.Add(instrument.Objective);
            }
            Value = Vector<double>.Build.Dense(_benchmarkObjectives.Count);
        }
    }
}