using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using QuantSA.Shared.Dates;
using QuantSA.Shared.State;

namespace QuantSA.CoreExtensions.Curves
{
    /// <summary>
    /// Collects the initial values from multiple <see cref="IRateCurveInstrument" />s and stores them by curve name.
    /// </summary>
    internal class InitialValueCollector
    {
        private static readonly ILog Log = QuantSAState.LogFactory.Get(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, List<Tuple<string, Date, double>>> _storage =
            new Dictionary<string, List<Tuple<string, Date, double>>>();

        internal InitialValueCollector(Date calibrationDate, IEnumerable<IRateCurveInstrument> instruments)
        {
            foreach (var instrument in instruments)
            {
                Log.Debug($"Getting curve initial values for {instrument.GetName()}");
                instrument.SetCalibrationDate(calibrationDate);
                Add(instrument.GetInitialValue());
            }
        }

        private void Add(Tuple<string, Date, double> value)
        {
            if (!_storage.TryGetValue(value.Item1, out var list))
            {
                list = new List<Tuple<string, Date, double>>();
                _storage[value.Item1] = list;
            }
            Log.Debug($"Adding {value.Item1}, {value.Item2}, {value.Item3}");
            list.Add(value);
        }

        internal (Date[] dates, double[] values) GetValues(IEnumerable<string> curveNames)
        {
            var combined = new List<Tuple<string, Date, double>>();
            foreach (var curveName in curveNames)
                if (_storage.TryGetValue(curveName, out var datesAndValues))
                    combined.AddRange(datesAndValues);

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
}