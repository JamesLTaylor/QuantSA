using System.Collections.Generic;
using QuantSA.Core.Serialization;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Primitives
{
    public abstract class Product : IProduct
    {
        /// <summary>
        /// The identifier of the product instance.  
        /// </summary>
        public string ID { get; protected set; } = "Not Set";

        public string Type { get; protected set; } = "Not Set";

        /// <summary>
        /// Set the value date of the contract.
        /// </summary>
        public abstract void SetValueDate(Date valueDate);

        /// <summary>
        /// Reset any previous index information, will be called before setting the indices.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// A list of all possible currencies that cashflows may occur in.  This is used to make 
        /// sure that there are simulators available to convert these to the value currency.
        /// </summary>
        /// <returns></returns>
        public abstract List<Currency> GetCashflowCurrencies();

        /// <summary>
        /// A list of all possible indices that can be required to get the cashflows on the product.
        /// </summary>
        /// <returns></returns>
        public abstract List<MarketObservable> GetRequiredIndices();

        /// <summary>
        /// The dates at which the provided index is required to calculate cashflows.  This will be called 
        /// repeatedly so if possible pre-calculate in <see cref="SetValueDate(Date)"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract List<Date> GetRequiredIndexDates(MarketObservable index);

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
        public abstract List<Date> GetCashflowDates(Currency ccy);

        /// <summary>
        /// Precursor to calling <see cref="GetCFs"/>.  Done in a separate step in case the indices come from
        /// multiple sources.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="indexValues"></param>
        public abstract void SetIndexValues(MarketObservable index, double[] indexValues);

        /// <summary>
        /// Call this after <see cref="SetIndexValues(MarketObservable, double[])"/> to get all the cashflows on 
        /// or AFTER the value date.
        /// </summary>
        /// <returns>A List of cashflows.  Under some circumstances it may be faster if these are ordered by 
        /// increasing time.</returns>
        public abstract List<Cashflow> GetCFs();

        /// <summary>
        /// Return a deep copy of the Product, without any market data.
        /// </summary>
        public virtual IProduct Clone()
        {
            return (Product) Cloner.Clone(this);
        }
    }
}