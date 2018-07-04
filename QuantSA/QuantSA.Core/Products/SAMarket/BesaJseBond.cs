using System;
using System.Collections.Generic;
using QuantSA.Shared.Dates;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.SAMarket
{
    /// <summary>
    /// The market standard bond traded on the JSE, formerly the BESA.
    /// </summary>
    public class BesaJseBond : ProductWrapper
    {
        private readonly List<Cashflow> cfs;
        public double annualCouponRate;

        /// <summary>
        /// The number of days before a coupon date that the bond starts trading ex.
        /// </summary>
        public int booksCloseDateDays;

        public int couponDay1, couponDay2;
        public int couponMonth1, couponMonth2;

        public Date maturityDate;
        public double notional;

        /// <summary>
        /// The settlement date for this bond.  It could affect the cashflows if it is after the
        /// next ex date.
        /// </summary>
        /// <remarks>
        /// We don't take the trade date since the usual assumption in QuantSA is 
        /// that products will only be valued if they have already been traded.
        /// </remarks>
        public Date settleDate;

        private BesaJseBond()
        {
        }

        public BesaJseBond(Date maturityDate, double notional, double annualCouponRate,
            int couponMonth1, int couponDay1, int couponMonth2, int couponDay2, Calendar zaCalendar, Currency ccy)
        {
            if (couponMonth1 > couponMonth2)
                throw new ArgumentException("couponMonth1 must relate to the first coupon in the year.");
            this.maturityDate = maturityDate;
            this.notional = notional;
            this.annualCouponRate = annualCouponRate;
            this.couponMonth1 = couponMonth1;
            this.couponDay1 = couponDay1;
            this.couponMonth2 = couponMonth2;
            this.couponDay2 = couponDay2;

            cfs = new List<Cashflow> {new Cashflow(maturityDate, notional, ccy)};
            Init();
        }

        public override List<Cashflow> GetCFs()
        {
            return cfs;
        }
    }
}