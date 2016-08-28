using General;
using System;
using System.Collections.Generic;

namespace MonteCarlo
{
    public abstract class Product
    {
        /// <summary>
        /// A list of all possible indices that can be required to get the cashflows on the product.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<MarketObservable> GetRequiredIndices();
        
        /// <summary>
        /// Dates at which the provided index is required to calculate cashflows.
        /// </summary>
        /// <param name="valueDate"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract int[] GetRequiredTimes(Date valueDate, MarketObservable index);

        /// <summary>
        /// Call this after <see cref="SetIndices(MarketObservable, double[])"/> to get all the cashflows on or AFTER the value date.
        /// </summary>
        /// <returns>2 column matrix of dates and amounts</returns>
        public abstract double[,] GetCFs();


        /// <summary>
        /// Precursor to calling <see cref="GetCFs"/>.  Done in a separate step in case the indices come from multiple sources.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="indices"></param>
        public abstract void SetIndices(MarketObservable index, double[] indices);

    }
}