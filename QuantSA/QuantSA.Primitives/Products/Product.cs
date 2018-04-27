using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.Dates;

namespace QuantSA.General
{
    /// <summary>
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
    [Serializable]
    public abstract class Product
    {
        /// <summary>
        /// The identifier of the product instance.  
        /// </summary>
        public string id { get; protected set; } = "Not Set";
        public string type { get; protected set; } = "Not Set";

        /// <summary>
        /// Return a deep copy of the Product, without any market data.
        /// </summary>
        public virtual Product Clone()
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream);
            return (Product)o;
        }


        /// <summary>
        /// Set the value date of the contract.
        /// </summary>
        public abstract void SetValueDate(Date valueDate);

        /// <summary>
        /// Reset any previous index information, will be called before setting the indices.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// A list of all possible currencies that cashlows may occur in.  This is used to make 
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
        /// repeatedly so if possible precalculate in <see cref="SetValueDate(Date)"/>.
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

    }

    public static class ProductExtensions {
        /// <summary>
        /// Makes a deep copy of the List of <see cref="Product"/>s
        /// </summary>
        /// <param name="originalPortfolio">The original portfolio.</param>
        /// <returns></returns>
        public static List<Product> Clone(this List<Product> originalPortfolio)
        {
            List<Product> newPortfolio = new List<Product>();
            foreach (Product p in originalPortfolio)
            {
                newPortfolio.Add(p.Clone());
            }
            return newPortfolio;
        }
    }

}