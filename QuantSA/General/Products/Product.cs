using System;
using System.Collections.Generic;

namespace QuantSA.General
{
    /// <summary>
    /// The call sequence for getting the cashflows of any product must be:
    /// 
    /// Product.Reset(...)
    /// Product.SetValueDate(...)
    /// Product.GetRequiredIndices(...)
    /// Then for each index:
    ///     Product.GetRequiredIndexDates(...)
    ///     Product.SetIndexValues(...) 
    /// GetCFs()
    /// 
    /// The calls to GetCashflowCurrencies and GetCashflowDates will only be used when there is a 
    /// multi currency valuation and exchange rates will need to be used in the valuation.
    /// </summary>
    public abstract class Product
    {
        /// <summary>
        /// The identifier of the product instance.  
        /// </summary>
        public string id { get; protected set; } = "Not Set";
        public string type { get; protected set; } = "Not Set";

        /// <summary>
        /// Sets the value date of the contract.
        /// </summary>
        public abstract void SetValueDate(Date valueDate);

        /// <summary>
        /// Resets any previous index information, will be called before setting the indices.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// A list of all possible currencies that cashlows may occur in.  This is used to make sure that there are simulators available to convert these to the value currency.
        /// </summary>
        /// <returns></returns>
        public abstract List<Currency> GetCashflowCurrencies();

        /// <summary>
        /// A list of all possible indices that can be required to get the cashflows on the product.
        /// </summary>
        /// <returns></returns>
        public abstract List<MarketObservable> GetRequiredIndices();

        /// <summary>
        /// Dates at which the provided index is required to calculate cashflows.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract List<Date> GetRequiredIndexDates(MarketObservable index);

        /// <summary>
        /// Dates at which the cashflows in a given currency are likely to take place.  Will be used to ensure that the numeraire and possible FX rates are available on these dates.
        /// </summary>
        /// <param name="ccy">Only return the dates for the cashflows in this provided currency.</param>
        /// <returns></returns>
        public abstract List<Date> GetCashflowDates(Currency ccy);

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