﻿using System;
using System.Collections.Generic;
using System.Linq;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products
{
    /// <summary>
    /// Wraps the general <see cref="Product" /> class in a simple case where there are cashflows
    /// in only one currency and all cashflows are always calculated.
    /// Requires fewer methods to be implemented.
    /// </summary>
    /// <seealso cref="Product" />
    public abstract class ProductWrapper : Product
    {
        private List<Date> _cfDates;
        private Currency _currency;

        private GetIndexValueDelegate _getIndexValueToUse;

        /// <summary>
        /// For each index, store the dates that are required and during a valuation
        /// also the values at each of those dates.  
        /// Stored in a dictionary of lists rather than a 
        /// dictionary of dictionaries since the order of the dates and values is
        /// important in <see cref="SetIndexValues(MarketObservable, double[])"/>.  
        /// It makes lookup slightly harder but is generally safer since there is no chance
        /// that values and dates may end up in a different order.  Also allows the values
        /// to be nulled which allows the code to check if they have been set before they 
        /// are used.
        /// </summary>
        private Dictionary<MarketObservable, List<Date>> _indexAndDates;

        private Dictionary<MarketObservable, List<double>> _indexAndValues;
        protected Date valueDate;

        protected ProductWrapper()
        {
            _indexAndDates = new Dictionary<MarketObservable, List<Date>>();
            _indexAndValues = new Dictionary<MarketObservable, List<double>>();
            _getIndexValueToUse = GetNormal;
        }

        /// <summary>
        /// Constructor with single currency.  The only constructor.
        /// </summary>
        /// <param name="currency"></param>
        protected ProductWrapper(Currency currency) : this()
        {
            _currency = currency;
        }

        /// <summary>
        /// Get the realized value of <paramref name="index"/> at <paramref name="date"/>
        /// </summary>
        /// <param name="index">The index whose value is required.</param>
        /// <param name="date">The date on which the value is required.</param>
        /// <returns></returns>
        protected double Get(MarketObservable index, Date date)
        {
            return _getIndexValueToUse(index, date);
        }

        /// <summary>
        /// The standard implementation of <see cref="Get(MarketObservable, Date)"/>
        /// </summary>
        /// <param name="index">The index whose value is required.</param>
        /// <param name="date">The date on which the value is required.</param>
        /// <returns></returns>
        private double GetNormal(MarketObservable index, Date date)
        {
            var location = _indexAndDates[index].FindIndex(d => d.Equals(date));
            if (_indexAndValues == null) throw new Exception("Index values can not be used before they have been set.");
            return _indexAndValues[index][location];
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
        public void Init()
        {
            _indexAndDates = new Dictionary<MarketObservable, List<Date>>();
            _indexAndValues = new Dictionary<MarketObservable, List<double>>();
            _getIndexValueToUse = GetNormal;
            _getIndexValueToUse = GetWithLogging;
            var cfs = GetCFs();
            SetCashflowDates(cfs.GetDates());
            _getIndexValueToUse = GetNormal;
            _currency = cfs[0].Currency;
        }


        /// <summary>
        /// Sets the required <see cref="MarketObservable"/>s for this product.  Allows the 
        /// valuation engine to know what it must provide to generate the cashflows.
        /// </summary>
        /// <param name="index">The required MarketObservable</param>
        /// <param name="date">The date at which this is required.</param>
        protected void SetRequired(MarketObservable index, Date date)
        {
            if (_indexAndDates.ContainsKey(index))
            {
                if (!_indexAndDates[index].Contains(date))
                    _indexAndDates[index].Add(date);
                // Leave index values unset so that there will be errors if they are used before they are set.
            }
            else
            {
                _indexAndDates[index] = new List<Date> {date};
                _indexAndValues[index] = null;
            }
        }

        /// <summary>
        /// Sets the dates on which cashflows take place.  In most circumstances these 
        /// can be deduced via a call to <see cref="Init"/>
        /// </summary>
        /// <param name="cfDates"></param>
        protected void SetCashflowDates(List<Date> cfDates)
        {
            _cfDates = cfDates;
        }

        private delegate double GetIndexValueDelegate(MarketObservable index, Date date);

        #region Product Implementation

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            if (ccy == _currency) return _cfDates;
            return new List<Date>();
        }

        public sealed override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> {_currency};
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            return _indexAndDates[index];
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return _indexAndDates.Keys.ToList();
        }

        public override void Reset()
        {
            foreach (var index in _indexAndDates.Keys) _indexAndValues[index] = null;
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            _indexAndValues[index] = indexValues.ToList();
        }

        public override void SetValueDate(Date valueDate)
        {
            this.valueDate = valueDate;
            Init();
        }

        #endregion
    }
}