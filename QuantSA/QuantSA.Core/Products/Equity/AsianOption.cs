using System;
using System.Collections.Generic;
using QuantSA.Core.Formulae;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Equity
{
    public class AsianOption : Product
    {


        public readonly Date _exerciseDate;
        private double[] stockpath;
        public readonly Share _share;
        public readonly PutOrCall _putOrCall;
        public readonly double _strike;
        private Date _valueDate;

        public AsianOption(Share share, PutOrCall putOrCall, double strike, Date exerciseDate)
        {
            _share = share;
            _putOrCall = putOrCall;
            _strike = strike;
            _exerciseDate = exerciseDate;
        }

        public override List<Cashflow> GetCFs()
        {
            double sum = 0;
            for (int i = 0; i < stockpath.GetLength(0); i++)
            {
                sum += stockpath[i];
            }
            double average = sum / stockpath.GetLength(0);
            var amount = Math.Max(0, (double)_putOrCall * (average - _strike));
            return new List<Cashflow> { new Cashflow(_exerciseDate, amount, _share.Currency) };
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable> { _share };
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            if (_valueDate <= _exerciseDate)
                return new List<Date> { _exerciseDate };
            return new List<Date>();
        }


        public override void SetValueDate(Date valueDate)
        {
            _valueDate = valueDate;
        }

        public override void Reset()
        {
            // Nothing to reset.
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> { _share.Currency };
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            return new List<Date> { _exerciseDate };
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            throw new NotImplementedException();
        }
    }
}
