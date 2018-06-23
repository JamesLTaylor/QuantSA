using System;
using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.General
{
    /// <summary>
    /// A set of cashflows that can be valued like any other product.
    /// </summary>
    [Serializable]
    public class CashLeg : Product
    {
        protected List<Cashflow> cfs;
        protected Date valueDate;

        protected CashLeg()
        {
        }

        public CashLeg(Date[] dates, double[] amounts, Currency[] currencies)
        {
            valueDate = null;
            cfs = new List<Cashflow>();
            for (var i = 0; i < amounts.Length; i++) cfs.Add(new Cashflow(dates[i], amounts[i], currencies[i]));
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            var currencies = new List<Currency>();
            foreach (var cf in cfs)
                if (!currencies.Contains(cf.Currency))
                    currencies.Add(cf.Currency);
            return currencies;
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            var dates = new List<Date>();
            foreach (var cf in cfs)
                if (cf.Date > valueDate)
                    dates.Add(cf.Date);
            return dates;
        }

        public override List<Cashflow> GetCFs()
        {
            var futureCFs = new List<Cashflow>();
            foreach (var cf in cfs)
                if (cf.Date > valueDate)
                    futureCFs.Add(cf);
            return futureCFs;
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            return new List<Date>();
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable>();
        }

        public override void Reset()
        {
            // Nothing to do.
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            // Nothing to do.
        }

        public override void SetValueDate(Date valueDate)
        {
            this.valueDate = valueDate;
        }
    }
}