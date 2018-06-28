using System;
using Accord.Math;
using QuantSA.Core.CurvesAndSurfaces;
using QuantSA.Shared.CurvesAndSurfaces;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;
using Normal = MathNet.Numerics.Distributions.Normal;

namespace QuantSA.General
{
    
    public class PCACurveSimulator
    {
        /// <summary>
        /// The principle components in rows.
        /// </summary>
        private readonly double[,] components;

        private readonly bool floorAtZero;
        private readonly double[] initialRates;
        private readonly double multiplier;
        private readonly Tenor[] tenors;
        private readonly bool useRelative;
        private readonly double[] vols;


        /// <summary>
        /// Constructs a PCA curve simulator
        /// </summary>
        /// <param name="anchorDate"></param>
        /// <param name="initialRates"></param>
        /// <param name="tenorMonths"></param>
        /// <param name="components">Can be passed in rows or columns.  The orientation will be 
        /// checked by comparing with the length of <paramref name="tenorMonths"/> </param>
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
                this.components = components.Transpose();
            else if (components.GetLength(0) < initialRates.Length && components.GetLength(1) == initialRates.Length)
                this.components = components;
            else
                throw new ArgumentException(
                    "The components data must either have the same number of columns as the length of the provided tenors if the components are provided in rows, or the same rows if they are provided in columns.");

            if (tenors.Length != initialRates.Length)
                throw new ArgumentException("The provided dates tenros and initial rates must be the same length.");
            if (vols.Length != this.components.GetLength(0))
                throw new ArgumentException("The number of vols must be the same as the number of components.");

            this.anchorDate = anchorDate;
            this.initialRates = initialRates;
            this.tenors = tenors;
            this.vols = vols;
            this.multiplier = multiplier;
            this.useRelative = useRelative;
            this.floorAtZero = floorAtZero;
        }

        public Date anchorDate { get; }


        /// <summary>
        /// Produce a vector of curves where the element at index i is a realization of a simulation at 
        /// simulationDates i.  If you require the rates directly use <see cref="GetSimulatedRates(Date[])"/>
        /// </summary>
        /// <param name="simulationDates">Dates on which the simulation is run.  Must all be greater than the 
        /// anchor date.</param>
        /// <returns></returns>
        public ICurve[] GetSimulatedCurves(Date[] simulationDates, Currency curveCcy = null)
        {
            if (curveCcy == null)
                curveCcy = Currency.ANY;
            var results = new ICurve[simulationDates.Length];
            var dist = new Normal();
            var previousDate = anchorDate;

            var previousRates = initialRates.Clone() as double[];
            var currentRates = new double[initialRates.Length];

            // Iterate through the simulation dates
            for (var simCounter = 0; simCounter < simulationDates.Length; simCounter++)
            {
                var currentDate = simulationDates[simCounter];
                var dt = (currentDate - previousDate) / 365.0;
                var sdt = Math.Sqrt(dt);
                var curveDates = new Date[initialRates.Length];

                // Random realizations to be used in simulation.
                var eps1 = dist.Sample();
                var eps2 = dist.Sample();
                var eps3 = dist.Sample();

                // Iterate thrrough the dates on the curve
                for (var i = 0; i < initialRates.Length; i++)
                {
                    curveDates[i] = simulationDates[simCounter].AddTenor(tenors[i]);
                    if (useRelative)
                    {
                        //TODO: add mean correction.
                        var exponent = components[0, i] * vols[0] * sdt * eps1 +
                                       components[1, i] * vols[1] * sdt * eps2 +
                                       components[2, i] * vols[2] * sdt * eps3;
                        currentRates[i] = previousRates[i] * Math.Exp(exponent);
                    }
                    else
                    {
                        var change = components[0, i] * vols[0] * sdt * eps1 + components[1, i] * vols[1] * sdt * eps2 +
                                     components[2, i] * vols[2] * sdt * eps3;
                        currentRates[i] = previousRates[i] + change;
                        if (floorAtZero) currentRates[i] = Math.Max(0.0, currentRates[i]);
                    }
                }

                currentRates = currentRates.Multiply(multiplier);
                results[simCounter] = new DatesAndRates(curveCcy, simulationDates[simCounter], curveDates, currentRates,
                    simulationDates[simCounter].AddMonths(360));
                previousRates = currentRates.Clone() as double[];
                previousDate = new Date(currentDate);
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulationDates">Dates on whihc the simulation is run.  Must all be greater than the anchor date.</param>
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