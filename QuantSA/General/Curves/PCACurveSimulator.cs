using MathNet.Numerics.Distributions;
using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA
{
    [Serializable]
    public class PCACurveSimulator
    {
        private int[] tenorMonths;
        /// <summary>
        /// The principle components in rows.
        /// </summary>
        private double[,] components;
        private double[] vols;        
        private double[] initialRates;
        private double multiplier;
        private bool useRelative;
        private bool floorAtZero;

        public Date anchorDate { get; private set; }


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
        public PCACurveSimulator(Date anchorDate, double[] initialRates, int[] tenorMonths, double[,] components, 
            double[] vols, double multiplier, bool useRelative, bool floorAtZero)
        {
            if (multiplier < 0.1 || multiplier > 10) throw new ArgumentException("multiplier should be close to 1.0 for the simulation to make sense");
            if (components.GetLength(0) == initialRates.Length && components.GetLength(1) == initialRates.Length)
                throw new ArgumentException("Simulator must not use all the components."); //Also makes the orientation ambiguous.
            else if (components.GetLength(0) == initialRates.Length && components.GetLength(1) < initialRates.Length)
            {
                // In columns, transpose
                this.components = components.Transpose();
            }
            else if (components.GetLength(0) < initialRates.Length && components.GetLength(1) == initialRates.Length)
            {
                // In rows, do nothing
                this.components = components;
            }
            else
                throw new ArgumentException("The components data must either have the same number of columns as the length of the provided tenors if the components are provided in rows, or the same rows if they are provided in columns.");

            if (tenorMonths.Length != initialRates.Length)
                throw new ArgumentException("The provided dates tenros and initial rates must be the same length.");
            if (vols.Length!=this.components.GetLength(0))
                throw new ArgumentException("The number of vols must be the same as the number of components.");

            this.anchorDate = anchorDate;
            this.initialRates = initialRates;
            this.tenorMonths = tenorMonths;            
            this.vols = vols;
            this.multiplier = multiplier;
            this.useRelative = useRelative;
            this.floorAtZero = floorAtZero;
        }


        /// <summary>
        /// Produce a vector of curves where the element at index i is a realization of a simulation at 
        /// simulationDates i.  If you require the rates directly use <see cref="GetSimulatedRates(Date[])"/>
        /// </summary>
        /// <param name="simulationDates">Dates on which the simulation is run.  Must all be greater than the 
        /// anchor date.</param>
        /// <returns></returns>
        public ICurve[] GetSimulatedCurves(Date[] simulationDates)
        {
            ICurve[] results = new ICurve[simulationDates.Length];
            MathNet.Numerics.Distributions.Normal dist = new MathNet.Numerics.Distributions.Normal();
            Date previousDate = anchorDate;

            double[] previousRates = initialRates.Clone() as double[];
            double[] currentRates = new double[initialRates.Length];

            // Iterate through the simulation dates
            for (int simCounter = 0; simCounter < simulationDates.Length; simCounter++)
            {
                Date currentDate = simulationDates[simCounter];
                double dt = (currentDate - previousDate) / 365.0;
                double sdt = Math.Sqrt(dt);
                Date[] curveDates = new Date[initialRates.Length];

                // Random realizations to be used in simulation.
                double eps1 = dist.Sample();
                double eps2 = dist.Sample();
                double eps3 = dist.Sample();

                // Iterate thrrough the dates on the curve
                for (int i = 0; i < initialRates.Length; i++)
                {
                    curveDates[i] = simulationDates[simCounter].AddMonths(tenorMonths[i]);
                    if (useRelative)
                    {
                        //TODO: add mean correction.
                        double exponent = components[0, i] * vols[0] * sdt * eps1 + components[1, i] * vols[1] * sdt * eps2 + components[2, i] * vols[2] * sdt * eps3;
                        currentRates[i] = previousRates[i] * Math.Exp(exponent);
                    }
                    else
                    {
                        double change = components[0, i] * vols[0] * sdt * eps1 + components[1, i] * vols[1] * sdt * eps2 + components[2, i] * vols[2] * sdt * eps3;
                        currentRates[i] = previousRates[i] + change;
                        if (floorAtZero) currentRates[i] = Math.Max(0.0, currentRates[i]);
                    }
                }

                currentRates = currentRates.Multiply(multiplier);
                results[simCounter] = new DatesAndRates(Currency.ANY, simulationDates[simCounter], curveDates, currentRates, simulationDates[simCounter].AddMonths(360));
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
        public double[,] GetSimulatedRates(Date[] simulationDates, int[] requiredTenorMonths)
        {
            double[,] rates = new double[simulationDates.Length, requiredTenorMonths.Length];
            ICurve[] curves = GetSimulatedCurves(simulationDates);
            // Iterate through the simulation dates

            for (int simCounter = 0; simCounter < simulationDates.Length; simCounter++)
            {
                for (int i = 0; i < requiredTenorMonths.Length; i++)
                {
                    Date curveDate = simulationDates[simCounter].AddMonths(tenorMonths[i]);
                    rates[simCounter, i] = curves[simCounter].InterpAtDate(curveDate);
                }
            }
            return rates;
        }

    }
}
