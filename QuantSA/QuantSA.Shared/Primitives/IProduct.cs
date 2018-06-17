using System.Collections.Generic;
using QuantSA.General;
using QuantSA.Shared.Dates;

namespace QuantSA.Shared.Primitives
{
    /// <summary>
    /// Any product, where products are defined entirely in terms of how their cashflows are
    /// calculated given realizations of <see cref="MarketObservable"/>s.
    /// <para/>
    /// The call sequence for getting the cashflows of any product must be:
    /// <para/>
    /// Product.Reset(...)
    /// <para/>
    /// Product.SetValueDate(...)
    /// <para/>
    /// Product.GetCashflowCurrencies(...)
    /// <para/>
    /// Product.GetRequiredIndices(...)
    /// <para/>
    /// Then inside the simulation loop
    /// <para/>
    /// Then for each index:
    /// <para/>
    /// -  Product.Reset(...)
    /// <para/>
    /// -  Product.GetRequiredIndexDates(...)
    /// <para/>
    /// -  Product.SetIndexValues(...) 
    /// <para/>
    /// GetCFs()
    /// <para/>
    /// 
    /// The calls to GetCashflowCurrencies and GetCashflowDates will only be used when there is a 
    /// multi currency valuation and exchange rates will need to be used in the valuation.
    /// </summary>
    public interface IProduct
    {
        /// <summary>
        /// Set the value date of the contract.
        /// </summary>
        void SetValueDate(Date valueDate);

        /// <summary>
        /// Reset any previous index information, will be called before setting the indices.
        /// </summary>
        void Reset();

        /// <summary>
        /// A list of all possible currencies that cashflows may occur in.  This is used to make 
        /// sure that there are simulators available to convert these to the value currency.
        /// </summary>
        /// <returns></returns>
        List<Currency> GetCashflowCurrencies();

        /// <summary>
        /// A list of all possible indices that can be required to get the cashflows on the product.
        /// </summary>
        /// <returns></returns>
        List<MarketObservable> GetRequiredIndices();

        /// <summary>
        /// The dates at which the provided index is required to calculate cashflows.  This will be called 
        /// repeatedly so if possible pre-calculate in <see cref="Product.SetValueDate"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        List<Date> GetRequiredIndexDates(MarketObservable index);

        /// <summary>
        /// The dates at which the cashflows in a given currency are likely to take place.  It will be used to ensure 
        /// that the numeraire and possible FX rates are available on these dates.
        /// 
        /// It will be called once before the contract has any MarketObservables set so if the timing of a 
        /// cashflow depends on the realized value of market data a best guess needs to be made
        /// here.  Some models may be able to provide FX and numeraire values at dates that were not originally 
        /// specified, via bridging or interpolation, but this is not enforced by the interfaces.
        /// </summary>
        /// <param name="ccy">Only return the dates for the cashflows in this provided currency.</param>
        /// <returns></returns>
        List<Date> GetCashflowDates(Currency ccy);

        /// <summary>
        /// Precursor to calling <see cref="Product.GetCFs"/>.  Done in a separate step in case the indices come from
        /// multiple sources.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="indexValues"></param>
        void SetIndexValues(MarketObservable index, double[] indexValues);

        /// <summary>
        /// Call this after <see cref="Product.SetIndexValues"/> to get all the cashflows on 
        /// or AFTER the value date.
        /// </summary>
        /// <returns>A List of cashflows.  Under some circumstances it may be faster if these are ordered by 
        /// increasing time.</returns>
        List<Cashflow> GetCFs();

        /// <summary>
        /// A deep clone. It must create a new product identical to this one with all data deep copied.
        /// </summary>
        /// <returns></returns>
        IProduct Clone();
    }
}