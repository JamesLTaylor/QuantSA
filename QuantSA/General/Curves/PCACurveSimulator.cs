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
        private double[,] components;
        private double[] vols;        
        private double[] initialRates;
        private double multiplier;

        public Date anchorDate { get; private set; }


        /// <summary>
        /// Constructs a PCA curve simulator
        /// </summary>
        /// <param name="tenorMonths"></param>
        /// <param name="components"></param>
        /// <param name="vols"></param>
        public PCACurveSimulator(Date anchorDate, double[] initialRates, int[] tenorMonths, double[,] components, double[] vols, double multiplier)
        {
            if (multiplier < 0.1 || multiplier > 10) throw new ArgumentException("multiplier should be close to 1.0 for the simulation to make sense");
            //TODO: Check that these are all the right size.            
            this.anchorDate = anchorDate;
            this.initialRates = initialRates;
            this.tenorMonths = tenorMonths;
            this.components = components;
            this.vols = vols;
            this.multiplier = multiplier;            
        }


        /// <summary>
        /// Produce a vector of curves where the element at index i is a realization of a simulation at simulationDates i.  If you require the rates directly use <see cref="GetSimulatedRates(Date[])"/>
        /// </summary>
        /// <param name="simulationDates">Dates on which the simulation is run.  Must all be greater than the anchor date.</param>
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
                    //TODO: Add months properly
                    curveDates[i] = new Date(simulationDates[0] + 30 * tenorMonths[i]);
                    //TODO: add mean correction.
                    double exponent = components[0, i] * vols[0] * sdt * eps1 + components[1, i] * vols[1] * sdt * eps2 + components[2, i] * vols[2] * sdt * eps3;
                    currentRates[i] = previousRates[i] * Math.Exp(exponent);
                }

                currentRates = currentRates.Multiply(multiplier);
                results[simCounter] = new DatesAndRates(simulationDates[simCounter], curveDates, currentRates, simulationDates[simCounter].AddMonths(360));
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
                    //TODO: Add months properly
                    Date curveDate = new Date(simulationDates[0] + 30 * tenorMonths[i]);
                    rates[simCounter, i] = curves[simCounter].InterpAtDate(curveDate);
                }
            }
            return rates;
        }

    }
}
