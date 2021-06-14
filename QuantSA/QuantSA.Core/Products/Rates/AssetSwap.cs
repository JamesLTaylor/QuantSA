using System;
using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;
using QuantSA.Core.Products.SAMarket;


namespace QuantSA.Core.Products.Rates
{
    public class AssetSwap : ProductWrapper
    {
        private readonly List<Cashflow> _cfs;

        public string RateIndex;

        public int booksCloseDateDays;
        public int couponDay1, couponDay2;
        public int couponMonth1, couponMonth2;

        public Date maturityDate;
        public double notional;

        public double annualCouponRate;
        public Calendar zaCalendar;
        public Currency ccy;

        public BesaJseBond underlyingBond;
        public Date settleDate;
        public int tenorfloat;
        public double payFixed;

        public AssetSwap(double _payFixed, string RateIndex, Date maturityDate, double notional, double annualCouponRate, int couponMonth1, int couponDay1,
            int couponMonth2, int couponDay2, int booksCloseDateDays, Calendar zaCalendar, Currency ccy)
        {
            //Bond 
            var bond = new BesaJseBond(maturityDate, notional, annualCouponRate, couponMonth1,
                   couponDay1, couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, ccy);
            underlyingBond = bond;

            //Tenor of floating leg
            var tenorFloatLeg = Convert.ToInt32(RateIndex.Substring(10, 1));
            tenorfloat = tenorFloatLeg;

            //pay Fixed
            var interim = _payFixed;
            payFixed = interim;

            _cfs = new List<Cashflow> { new Cashflow(maturityDate, notional, ccy) };
            Init();

        }

        public override List<Cashflow> GetCFs()
        {
            return _cfs;
        }

    }
}
