using System;
using System.Collections.Generic;
using QuantSA.Primitives.Dates;

namespace QuantSA.Primitives.Products.Rates
{
    /// <summary>
    /// A set of cashflows that can be valued like any other product.
    /// </summary>
    [Serializable]
    public class CashLeg : Product
    {
        protected List<Cashflow> cfs;
        protected Date valueDate;

        protected CashLeg() { }

        public CashLeg(Date[] dates, double[] amounts, Currency[] currencies)
        {
            valueDate = null;
            cfs = new List<Cashflow>();
            for (int i=0; i< amounts.Length; i++)
            {
                cfs.Add(new Cashflow(dates[i], amounts[i], currencies[i]));
            }
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            List<Currency> currencies = new List<Currency>();     
            foreach (Cashflow cf in cfs)
            {
                if (!currencies.Contains(cf.currency)) currencies.Add(cf.currency);
            }
            return currencies;            
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            List<Date> dates = new List<Date>();
            foreach (Cashflow cf in cfs)
            {
                if (cf.date> valueDate) dates.Add(cf.date);
            }
            return dates;            
        }
        
        public override List<Cashflow> GetCFs()
        {
            List<Cashflow> futureCFs = new List<Cashflow>();
            foreach (Cashflow cf in cfs)
            {
                if (cf.date > valueDate) futureCFs.Add(cf);
            }
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
