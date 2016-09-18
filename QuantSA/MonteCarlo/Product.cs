using QuantSA;
using System;
using System.Collections.Generic;

namespace MonteCarlo
{
    /// <summary>
    /// The call sequence for getting the cahsflows of any product must be:
    /// 
    /// Product.SetValueDateAndReset(...)
    /// Product.GetRequiredIndices(...)
    /// Then for each index:
    ///     Product.GetRequiredIndexDates(...)
    ///     Product.SetIndexValues(...) 
    /// GetCFs()
    /// </summary>
    public abstract class Product
    {
        /// <summary>
        /// Sets the value date of the contract.
        /// </summary>
        public abstract void SetValueDate(Date valueDate);

        /// <summary>
        /// Resets any previous index information, will be called before setting the indices.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// A list of all possible indices that can be required to get the cashflows on the product.
        /// </summary>
        /// <returns></returns>
        public abstract List<MarketObservable> GetRequiredIndices();

        /// <summary>
        /// Dates at which the provided index is required to calculate cashflows.
        /// </summary>
        /// <param name="valueDate"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract List<Date> GetRequiredIndexDates(MarketObservable index);

        /// <summary>
        /// Precursor to calling <see cref="GetCFs"/>.  Done in a separate step in case the indices come from multiple sources.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="indexValues"></param>
        public abstract void SetIndexValues(MarketObservable index, double[] indexValues);

        /// <summary>
        /// Call this after <see cref="SetIndexValues(MarketObservable, double[])"/> to get all the cashflows on or AFTER the value date.
        /// </summary>
        /// <returns>2 column matrix of dates and amounts</returns>
        public abstract List<Cashflow> GetCFs();

    }
}