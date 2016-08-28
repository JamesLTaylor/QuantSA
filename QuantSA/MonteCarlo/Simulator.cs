using System;

namespace MonteCarlo
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
        /// Set the times at which the index will be required.  These are set up frony since multiple
        /// products may use the same simulator and there may be a way to efficiently combine the required
        /// times.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="requiredTimes"></param>
        public abstract void SetRequiredTimes(MarketObservable index, int[] requiredTimes);

        /// <summary>
        /// Run the simulation and internally store the indices that will be required.  Should only be called 
        /// after <see cref="SetRequiredTimes(MarketObservable, double[])"/>
        /// </summary>
        /// <param name="simNumber">The simulation number.  Required if any variance reductions technigues are used.</param>
        public abstract void RunSimulation(int simNumber);

        /// <summary>
        /// Return the simulated values at the required times.  Should only be called after <see cref="RunSimulation(int, int)"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="requiredTimes"></param>
        /// <returns></returns>
        public abstract double[] GetIndices(MarketObservable index, int[] requiredTimes);

        /// <summary>
        /// Clear any stored data so that the object can be reused.
        /// </summary>
        public abstract void Reset();
    }
}