using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.General
{
    public class EuropeanOption : Product
    {
        private readonly Date exerciseDate;
        private double fwdPrice;
        private readonly Share share;
        private readonly double strike;
        private Date valueDate;

        public EuropeanOption(Share share, double strike, Date exerciseDate)
        {
            this.share = share;
            this.strike = strike;
            this.exerciseDate = exerciseDate;
        }

        public override List<Cashflow> GetCFs()
        {
            var amount = Math.Max(0, fwdPrice - strike);
            return new List<Cashflow> {new Cashflow(exerciseDate, amount, share.currency)};
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable> {share};
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            if (valueDate <= exerciseDate)
                return new List<Date> {exerciseDate};
            return new List<Date>();
        }

        public override void SetIndexValues(MarketObservable index, double[] indices)
        {
            fwdPrice = indices[0];
        }

        public override void SetValueDate(Date valueDate)
        {
            this.valueDate = valueDate;
        }

        public override void Reset()
        {
            // Nothing to reset.
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> {share.currency};
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            return new List<Date> {exerciseDate};
        }
    }
}