using QuantSA.General;
using System.Collections.Generic;

namespace QuantSA.Valuation
{
    public abstract class Simulator
    {
        /// <summary>
        /// Identify if the the simulator is able to simulate the provided index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract bool ProvidesIndex(MarketObservable index);

        /// <summary>
        /// Clear any stored data so that the object can be reused for a new simulation.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Set the times at which the index will be required.  These are set up front since multiple
        /// products may use the same simulator and there may be a way to efficiently combine the required
        /// times.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="requiredDates">The date at which <paramref name="index"/> will be required.  This method can be called multiple times for the same index but you 
        /// are guaranteed that <see cref="Prepare"/> will be called before a simulation is requested.</param>        
        public abstract void SetRequiredDates(MarketObservable index, List<Date> requiredDates);


        /// <summary>
        /// Final call before the simulation takes place.  Opportunity for the simulator do such things as:
        ///  * sort and removed duplicate required dates
        ///  * add extra simulation dates internally
        /// </summary>
        public abstract void Prepare();

        /// <summary>
        /// Run the simulation and internally store the indices that will be required.  Should only be called 
        /// after <see cref="SetRequiredDates(MarketObservable, List{Date})"/>
        /// </summary>
        /// <param name="simNumber">The simulation number.  May be required for example if any variance reductions technigues are used.</param>
        public abstract void RunSimulation(int simNumber);

        /// <summary>
        /// Return the simulated values at the required times.  Should only be called after <see cref="RunSimulation(int, int)"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="requiredTimes"></param>
        /// <returns></returns>
        public abstract double[] GetIndices(MarketObservable index, List<Date> requiredDates);

        /// <summary>
        /// Gets the underlying factors in the simulation.  This will be used to perform regressions
        /// of product discounted cashflows against theses values.
        /// 
        /// The method must always return the same number of factors in the same order.
        /// 
        /// Regression only makes sense for simulators with a low number of factors.  A Lognormal Forward
        /// Model with 120 brownian motions would not be practical and some dimension reduction would be 
        /// required. 
        /// </summary>
        /// <param name="date">The date at which the factors are required.</param>
        /// <returns></returns>
        public virtual double[] GetUnderlyingFactors(Date date)
        {
            //TODO: URGENT: Remove this implementation and make abstract.
            return new double[] { 1 };
        }

    }
}