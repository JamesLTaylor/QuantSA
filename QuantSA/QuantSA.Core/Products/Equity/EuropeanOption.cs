using System;
using System.Collections.Generic;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Equity
{
    public class EuropeanOption : Product
    {
        private readonly Date _exerciseDate;
        private double _fwdPrice;
        private readonly Share _share;
        private readonly double _strike;
        private Date _valueDate;

        public EuropeanOption(Share share, double strike, Date exerciseDate)
        {
            this._share = share;
            this._strike = strike;
            this._exerciseDate = exerciseDate;
        }

        public override List<Cashflow> GetCFs()
        {
            var amount = Math.Max(0, _fwdPrice - _strike);
            return new List<Cashflow> {new Cashflow(_exerciseDate, amount, _share.Currency)};
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable> {_share};
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            if (_valueDate <= _exerciseDate)
                return new List<Date> {_exerciseDate};
            return new List<Date>();
        }

        public override void SetIndexValues(MarketObservable index, double[] indices)
        {
            _fwdPrice = indices[0];
        }

        public override void SetValueDate(Date valueDate)
        {
            this._valueDate = valueDate;
        }

        public override void Reset()
        {
            // Nothing to reset.
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> {_share.Currency};
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            return new List<Date> {_exerciseDate};
        }
    }
}