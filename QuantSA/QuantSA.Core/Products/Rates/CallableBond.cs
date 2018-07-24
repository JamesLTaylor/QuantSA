using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using QuantSA.Core.Primitives;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Rates
{
    public class CallableBond : ProductWithEarlyExercise
    {
        private readonly Currency _currency;
        private readonly double _coupon;
        private readonly Date[] _couponDates;
        private readonly Date _firstCouponDate;
        private readonly double _notional;
        private List<Date> _exDates;
        private List<IProduct> _exProducts;

        [JsonIgnore] private Date _valueDate;

        public CallableBond(Currency currency)
        {
            _currency = currency;
            _firstCouponDate = new Date(2016, 12, 30);
            _couponDates = Enumerable.Range(1, 10).Select(i => _firstCouponDate.AddMonths(6 * i)).ToArray();
            _coupon = 0.185;
            _notional = 100;
            SetDates();
        }

        private void SetDates()
        {
            _exDates = new List<Date>();
            foreach (var couponDate in _couponDates)
                if (couponDate > _firstCouponDate)
                    _exDates.Add(couponDate);

            _exProducts = new List<IProduct>();
            foreach (var couponDate in _couponDates)
                if (couponDate > _firstCouponDate)
                    _exProducts.Add(new CashLeg(new[] {couponDate.AddTenor(Tenor.FromDays(1))}, new[] {-_notional},
                        new[] {_currency}));
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> {_currency};
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            return _couponDates.ToList();
        }

        public override List<Cashflow> GetCFs()
        {
            var cfs = new List<Cashflow>();
            for (var i = 0; i < _couponDates.Length; i++)
                if (_couponDates[i] > _valueDate)
                {
                    cfs.Add(new Cashflow(_couponDates[i], -_coupon * 0.5 * _notional, _currency));
                    if (i == _couponDates.Length - 1) cfs.Add(new Cashflow(_couponDates[i], -_notional, _currency));
                }

            return cfs;
        }

        public override List<Date> GetExerciseDates()
        {
            return _exDates;
        }

        public override int GetPostExProductAtDate(Date exDate)
        {
            for (var i = 0; i < _exDates.Count; i++)
                if (exDate == _exDates[i])
                    return i;
            throw new Exception(exDate + " is not an exercise date.");
        }

        public override List<IProduct> GetPostExProducts()
        {
            return _exProducts;
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            return new List<Date>();
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable>();
        }

        public override bool IsLongOptionality(Date exDate)
        {
            return true;
        }

        public override void Reset()
        {
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
        }

        public override void SetValueDate(Date valueDate)
        {
            _valueDate = valueDate;
        }
    }
}