using System;
using System.Collections.Generic;
using System.Linq;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.General.Products.Rates
{
    [Serializable]
    public class CallableBond : ProductWithEarlyExercise
    {
        private readonly double coupon;
        private readonly Date[] couponDates;
        private List<Date> exDates;
        private List<Product> exProducts;
        private readonly Date firstCouponDate;
        private Date maturityDate;
        private readonly double notional;

        private Date valueDate;

        public CallableBond()
        {
            firstCouponDate = new Date(2016, 12, 30);
            couponDates = Enumerable.Range(1, 10).Select(i => firstCouponDate.AddMonths(6 * i)).ToArray();
            maturityDate = couponDates[couponDates.Length - 1];
            coupon = 0.185;
            notional = 100;
            setDates();
        }

        private void setDates()
        {
            exDates = new List<Date>();
            for (var i = 0; i < couponDates.Length; i++)
                if (couponDates[i] > firstCouponDate)
                    exDates.Add(couponDates[i]);
            exProducts = new List<Product>();
            for (var i = 0; i < couponDates.Length; i++)
                if (couponDates[i] > firstCouponDate)
                    exProducts.Add(new CashLeg(new[] {couponDates[i].AddTenor(Tenor.Days(1))}, new[] {-notional},
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

        public override List<Product> GetPostExProducts()
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