using System;
using System.Collections.Generic;
using System.Linq;
using QuantSA.Core.Primitives;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Rates
{
    [Serializable]
    public class CallableBond : ProductWithEarlyExercise
    {
        private readonly double coupon;
        private readonly Date[] couponDates;
        private readonly Date firstCouponDate;
        private readonly double notional;
        private List<Date> exDates;
        private List<IProduct> exProducts;

        private Date valueDate;

        public CallableBond()
        {
            firstCouponDate = new Date(2016, 12, 30);
            couponDates = Enumerable.Range(1, 10).Select(i => firstCouponDate.AddMonths(6 * i)).ToArray();
            coupon = 0.185;
            notional = 100;
            SetDates();
        }

        private void SetDates()
        {
            exDates = new List<Date>();
            foreach (var couponDate in couponDates)
                if (couponDate > firstCouponDate)
                    exDates.Add(couponDate);

            exProducts = new List<IProduct>();
            foreach (var couponDate in couponDates)
                if (couponDate > firstCouponDate)
                    exProducts.Add(new CashLeg(new[] {couponDate.AddTenor(Tenor.FromDays(1))}, new[] {-notional},
                        new[] {Currency.ZAR}));
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> {Currency.ZAR};
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            return couponDates.ToList();
        }

        public override List<Cashflow> GetCFs()
        {
            var cfs = new List<Cashflow>();
            for (var i = 0; i < couponDates.Length; i++)
                if (couponDates[i] > valueDate)
                {
                    cfs.Add(new Cashflow(couponDates[i], -coupon * 0.5 * notional, Currency.ZAR));
                    if (i == couponDates.Length - 1) cfs.Add(new Cashflow(couponDates[i], -notional, Currency.ZAR));
                }

            return cfs;
        }

        public override List<Date> GetExerciseDates()
        {
            //setDates();
            return exDates;
        }

        public override int GetPostExProductAtDate(Date exDate)
        {
            for (var i = 0; i < exDates.Count; i++)
                if (exDate == exDates[i])
                    return i;
            throw new Exception(exDate + " is not an exercise date.");
        }

        public override List<IProduct> GetPostExProducts()
        {
            //setDates();
            return exProducts;
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
            //
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            //
        }

        public override void SetValueDate(Date valueDate)
        {
            this.valueDate = valueDate;
            //setDates();
        }
    }
}