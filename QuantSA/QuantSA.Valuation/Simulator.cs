using QuantSA.General;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using QuantSA.Shared.Dates;

namespace QuantSA.Valuation
{
    /// <summary>
    /// The fundemental class in QuantSA valuations.  Implementations of this are responsible primarily 
    /// for producing realizations of <see cref="MarketObservable"/>s.
    /// <para/>
    /// Implementations also need to provide underlying variables that can be used in regressions for 
    /// forward values.  See QuantSA.pdf.
    /// </summary>
    [Serializable]
    public abstract class Simulator
    {

        /// <summary>
        /// Clones this instance by serializing and deserializing the object.  If there is any issue with serializing
        /// an implementation of a Simulator then this method should be overridden.
        /// </summary>
        /// <returns></returns>
        public virtual Simulator Clone()
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream);
            return (Simulator)o;
        }

        /// <summary>
        /// Identify if the the simulator is able to simulate the provided index.
        /// </summary>
        /// <param name="index">The index that is tested for.</param>
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
        ///  * sort and remove duplicate required dates
        ///  * add extra simulation dates internally
        /// </summary>
        public abstract void Prepare();

        /// <summary>
        /// Run the simulation and internally store the indices that will be required.  Will only be called 
        /// after <see cref="SetRequiredDates(MarketObservable, List{Date})"/>
        /// </summary>
        /// <param name="simNumber">The simulation number.  May be required for example if any variance 
        /// reduction techniques are used.</param>
        public abstract void RunSimulation(int simNumber);

        /// <summary>
        /// Return the simulated values at the required times.  Will only be called after 
        /// <see cref="RunSimulation"/>
        /// </summary>
        /// <param name="index">The index that is required.</param>
        /// <param name="requiredDates">The dates on which the index is required.</param>
        /// <returns>An array of doubles the same length as <paramref name="requiredDates"/></returns>
        public abstract double[] GetIndices(MarketObservable index, List<Date> requiredDates);

        /// <summary>
        /// Gets the underlying factors in the simulation.  Will be used to perform regressions
        /// of product discounted cashflows against theses values.
        /// 
        /// The method must always return the same number of factors in the same order.
        /// 
        /// Regression only makes sense for simulators with a low number of factors.  For example a Lognormal forward
        /// model with 120 brownian motions would not be practical and some dimension reduction would be 
        /// required. 
        /// </summary>
        /// <param name="date">The date at which the factors are required.</param>
        /// <returns></returns>
        public abstract double[] GetUnderlyingFactors(Date date);
    }
}