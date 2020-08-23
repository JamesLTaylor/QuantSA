using System;
using Accord.Math;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Shared.CurvesAndSurfaces;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;
using Normal = MathNet.Numerics.Distributions.Normal;

namespace QuantSA.Core.CurveTools
{
    public class PCACurveSimulator
    {
        /// <summary>
        /// The principle components in rows.
        /// </summary>
        private readonly double[,] _components;

        private readonly bool _floorAtZero;
        private readonly double[] _initialRates;
        private readonly double _multiplier;
        private readonly Tenor[] _tenors;
        private readonly bool _useRelative;
        private readonly double[] _vols;


        /// <summary>
        /// Constructs a PCA curve simulator
        /// </summary>
        /// <param name="anchorDate"></param>
        /// <param name="initialRates"></param>
        /// <param name="tenors"></param>
        /// <param name="components">Can be passed in rows or columns.  The orientation will be 
        /// checked by comparing with the length of <paramref name="tenors"/> </param>
        /// <param name="vols"></param>
        /// <param name="multiplier"></param>
        /// <param name="useRelative"></param>
        /// <param name="floorAtZero"></param>
        public PCACurveSimulator(Date anchorDate, double[] initialRates, Tenor[] tenors, double[,] components,
            double[] vols, double multiplier, bool useRelative, bool floorAtZero)
        {
            if (multiplier < 0.1 || multiplier > 10)
                throw new ArgumentException("multiplier should be close to 1.0 for the simulation to make sense");
            if (components.GetLength(0) == initialRates.Length && components.GetLength(1) == initialRates.Length)
                throw new ArgumentException(
                    "Simulator must not use all the components."); //Also makes the orientation ambiguous.
            if (components.GetLength(0) == initialRates.Length && components.GetLength(1) < initialRates.Length)
                _components = components.Transpose();
            else if (components.GetLength(0) < initialRates.Length && components.GetLength(1) == initialRates.Length)
                _components = components;
            else
                throw new ArgumentException(
                    "The components data must either have the same number of columns as the length of the provided tenors if the components are provided in rows, or the same rows if they are provided in columns.");

            if (tenors.Length != initialRates.Length)
                throw new ArgumentException("The provided dates tenors and initial rates must be the same length.");
            if (vols.Length != _components.GetLength(0))
                throw new ArgumentException("The number of vols must be the same as the number of components.");

            AnchorDate = anchorDate;
            _initialRates = initialRates;
            _tenors = tenors;
            _vols = vols;
            _multiplier = multiplier;
            _useRelative = useRelative;
            _floorAtZero = floorAtZero;
        }

        private Date AnchorDate { get; }


        /// <summary>
        /// Produce a vector of curves where the element at index i is a realization of a simulation at 
        /// simulationDates i.  If you require the rates directly use <see cref="GetSimulatedRates(Date[],Tenor[])"/>
        /// </summary>
        /// <param name="simulationDates">Dates on which the simulation is run.  Must all be greater than the 
        /// anchor date.</param>
        /// <returns></returns>
        public ICurve[] GetSimulatedCurves(Date[] simulationDates)
        {
            var results = new ICurve[simulationDates.Length];
            var dist = new Normal();
            var previousDate = AnchorDate;

            var previousRates = _initialRates.Clone() as double[];
            var currentRates = new double[_initialRates.Length];

            // Iterate through the simulation dates
            for (var simCounter = 0; simCounter < simulationDates.Length; simCounter++)
            {
                var currentDate = simulationDates[simCounter];
                var dt = (currentDate - previousDate) / 365.0;
                var sdt = Math.Sqrt(dt);
                var curveDates = new Date[_initialRates.Length];

                // Random realizations to be used in simulation.
                var eps1 = dist.Sample();
                var eps2 = dist.Sample();
                var eps3 = dist.Sample();

                // Iterate through the dates on the curve
                for (var i = 0; i < _initialRates.Length; i++)
                {
                    curveDates[i] = simulationDates[simCounter].AddTenor(_tenors[i]);
                    if (_useRelative)
                    {
                        //TODO: add mean correction.
                        var exponent = _components[0, i] * _vols[0] * sdt * eps1 +
                                       _components[1, i] * _vols[1] * sdt * eps2 +
                                       _components[2, i] * _vols[2] * sdt * eps3;
                        currentRates[i] = previousRates[i] * Math.Exp(exponent);
                    }
                    else
                    {
                        var change = _components[0, i] * _vols[0] * sdt * eps1 + _components[1, i] * _vols[1] * sdt * eps2 +
                                     _components[2, i] * _vols[2] * sdt * eps3;
                        currentRates[i] = previousRates[i] + change;
                        if (_floorAtZero) currentRates[i] = Math.Max(0.0, currentRates[i]);
                    }
                }

                currentRates = currentRates.Multiply(_multiplier);
                results[simCounter] = new DatesAndRates(new Currency("IGNORE"), simulationDates[simCounter], curveDates, currentRates,
                    simulationDates[simCounter].AddMonths(360));
                previousRates = currentRates.Clone() as double[];
                previousDate = new Date(currentDate);
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulationDates">Dates on which the simulation is run.  Must all be greater than the anchor date.</param>
        /// <param name="requiredTenors"></param>
        /// <returns></returns>
        public double[,] GetSimulatedRates(Date[] simulationDates, Tenor[] requiredTenors)
        {
            var rates = new double[simulationDates.Length, requiredTenors.Length];
            var curves = GetSimulatedCurves(simulationDates);
            // Iterate through the simulation dates

            for (var simCounter = 0; simCounter < simulationDates.Length; simCounter++)
            for (var i = 0; i < requiredTenors.Length; i++)
            {
                var curveDate = simulationDates[simCounter].AddTenor(requiredTenors[i]);
                rates[simCounter, i] = curves[simCounter].InterpAtDate(curveDate);
            }

            return rates;
        }
    }
}