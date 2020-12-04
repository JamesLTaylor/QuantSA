using System;
using System.Collections.Generic;
using System.Text;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.SAMarket
{
    public class JSEBondForward : ProductWrapper
    {
        /// <summary> 
        /// Bond details
        /// <summary>
        private readonly List<Cashflow> _cfs;
        public readonly double annualCouponRate;

        // The number of days before a coupon date that the bond starts trading ex.
        public int booksCloseDateDays;

        public int couponDay1, couponDay2;
        public int couponMonth1, couponMonth2;

        public Date maturityDate;
        public double notional;

        // The settlement date for the underlying bond.  
        public Date settleDate;

        /// <summary> 
        /// Bond forward date
        /// <summary>
        public Date forwardDate;

        public BesaJseBond underlyingBond;

        public JSEBondForward(Date forwardDate, Date maturityDate, double notional, double annualCouponRate, int couponMonth1, int couponDay1, 
            int couponMonth2, int couponDay2, int booksCloseDateDays, Calendar zaCalendar, Currency ccy)
        {
            if (forwardDate > maturityDate)
                throw new ArgumentException("forward date must be before maturity date.");

            var bond = new BesaJseBond(maturityDate, notional, annualCouponRate, couponMonth1, couponDay1, 
                couponMonth2, couponDay2, booksCloseDateDays, zaCalendar, ccy);

            underlyingBond = bond;
            this.forwardDate = forwardDate;
        }

        public override List<Cashflow> GetCFs()
        {
            throw new NotImplementedException();
        }
    }
}
