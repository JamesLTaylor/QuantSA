using System;
using System.Collections.Generic;
using System.Linq;
using QuantSA.Primitives.Dates;

namespace QuantSA.Primitives.Products
{
    /// <summary>
    /// Wraps the general <see cref="Product" /> class in a simple case where there are cashflows
    /// in only one currency and all cashflows are always calculated.
    /// Requires fewer methods to be implemented.
    /// </summary>
    /// <seealso cref="Product" />
    [Serializable]
    public abstract class ProductWrapper : Product
    {
        delegate double GetIndexValueDelegate(MarketObservable index, Date date);

        GetIndexValueDelegate getIndexValueToUse;
        Currency currency;
        protected Date valueDate;
        List<Date> cfDates;

        /// <summary>
        /// For each index, store the dates that are required and during a valuation
        /// also the values at each of those dates.  
        /// Stored in a dictionary of lists rather than a 
        /// disctionary of dictionaries since the order of the dates and values is
        /// important in <see cref="SetIndexValues(MarketObservable, double[])"/>.  
        /// It makes lookup slightly harder but is generally safer since there is no chance
        /// that values and dates may end up in a different order.  Also allows the values
        /// to be nulled which allows the code to check if they have been set before they 
        /// are used.
        /// </summary>
        Dictionary<MarketObservable, List<Date>> indexAndDates;
        Dictionary<MarketObservable, List<double>> indexAndValues;

        protected ProductWrapper()
        {            
        }

        /// <summary>
        /// Constructor with single currency.  The only constructor.
        /// </summary>
        /// <param name="currency"></param>
        protected ProductWrapper(Currency currency)
        {
            this.currency = currency;
            indexAndDates = new Dictionary<MarketObservable, List<Date>>();
            indexAndValues = new Dictionary<MarketObservable, List<double>>();
            getIndexValueToUse = new GetIndexValueDelegate(GetNormal);
        }

        /// <summary>
        /// Get the realized value of <paramref name="index"/> at <paramref name="date"/>
        /// </summary>
        /// <param name="index">The index whose value is required.</param>
        /// <param name="date">The date on which the value is required.</param>
        /// <returns></returns>
        protected double Get(MarketObservable index, Date date)
        {
            return getIndexValueToUse(index, date);
        }

        /// <summary>
        /// The standard implementation of <see cref="Get(MarketObservable, Date)"/>
        /// </summary>
        /// <param name="index">The index whose value is required.</param>
        /// <param name="date">The date on which the value is required.</param>
        /// <returns></returns>
        private double GetNormal(MarketObservable index, Date date)
        {
            int location = indexAndDates[index].FindIndex(d => d.Equals(date));
            if (indexAndValues == null) throw new Exception("Index values can not be used before they have been set.");
            return indexAndValues[index][location];
        }

        /// <summary>
        /// A special implementation of <see cref="Get(MarketObservable, Date)"/>, only used by <see cref="Init"/>.
        /// 
        /// Logs which indices and dates are requested.  Under some circumstances this can allow
        /// a developer to initialize a <see cref="ProductWrapper"/> without calling 
        /// <see cref="SetRequired(MarketObservable, Date)"/> or <see cref="SetCashflowDates(List{Date})"/>.
        /// 
        /// </summary>
        /// <param name="index">The index whose value is required.</param>
        /// <param name="date">The date on which the value is required.</param>
        /// <returns></returns>
        private double GetWithLogging(MarketObservable index, Date date)
        {
            SetRequired(index, date);
            return 0.1;
        }

        /// <summary>
        /// Uses the overload <see cref="Product.GetCFs"/> to work out which <see cref="MarketObservable"/>s
        /// are required and at which times the cashflows take place.  All the MarketObservable values
        /// are set to 0.1 so the getCFs method must work fine for this value even if it is unnatural.
        /// </summary>
        protected void Init()
        {
            indexAndDates = new Dictionary<MarketObservable, List<Date>>();
            indexAndValues = new Dictionary<MarketObservable, List<double>>();
            getIndexValueToUse = new GetIndexValueDelegate(GetNormal);
            getIndexValueToUse = new GetIndexValueDelegate(GetWithLogging);
            List<Cashflow> cfs = GetCFs();
            SetCashflowDates(cfs.GetDates());
            getIndexValueToUse = new GetIndexValueDelegate(GetNormal);
            currency = cfs[0].currency;
        }


        /// <summary>
        /// Sets the required <see cref="MarketObservable"/>s for this product.  Allows the 
        /// valuation engine to know what it must provide to generate the cashflows.
        /// </summary>
        /// <param name="index">The required MarketObservable</param>
        /// <param name="date">The date at which this is required.</param>
        protected void SetRequired(MarketObservable index, Date date)
        {
            if (indexAndDates.ContainsKey(index))
            {
                if (!indexAndDates[index].Contains(date))
                    indexAndDates[index].Add(date);                
                // Leave index values unset so that there will be errors if they are used before they are set.
            }
            else
            {
                indexAndDates[index] = new List<Date> { date };
                indexAndValues[index] = null;
            }
        }

        /// <summary>
        /// Sets the dates on which cashflows take place.  In most circumstances these 
        /// can be deduced via a call to <see cref="Init"/>
        /// </summary>
        /// <param name="cfDates"></param>
        protected void SetCashflowDates(List<Date> cfDates)
        {
            this.cfDates = cfDates;
        }

        #region Product Implementation
        public override List<Date> GetCashflowDates(Currency ccy)
        {
            if (ccy == currency) return cfDates;
            else return new List<Date>();
        }

        public sealed override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> { currency };
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            return indexAndDates[index];
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return indexAndDates.Keys.ToList();
        }

        public override void Reset()
        {
            foreach (MarketObservable index in indexAndDates.Keys)
            {
                indexAndValues[index] = null;
            }
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            indexAndValues[index] = indexValues.ToList();
        }

        public override void SetValueDate(Date valueDate)
        {
            this.valueDate = valueDate;
        }
        #endregion
    }
}
