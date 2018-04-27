using System;
using System.Collections.Generic;
using QuantSA.Primitives.Dates;

namespace QuantSA.Primitives.Products.SAMarket
{
    /// <summary>
    /// The market standard bond traded on the JSE, formerly the BESA.
    /// </summary>
    public partial class BesaJseBond : ProductWrapper, IProvidesResultStore
    {

        public Date maturityDate;
        public double notional;
        public double annualCouponRate;
        public int couponMonth1, couponMonth2;
        public int couponDay1, couponDay2;

        /// <summary>
        /// The number of days before a coupon date that the bond starts trading ex.
        /// </summary>
        public int booksCloseDateDays;
        /// <summary>
        /// The settlement date for this bond.  It could affect the cashflows if it is after the
        /// next ex date.
        /// </summary>
        /// <remarks>
        /// We don't take the trade date since the usual assumption in QuantSA is 
        /// that products will only be valued if they have already been traded.
        /// </remarks>
        public Date settleDate;

        List<Cashflow> cfs;


        public BesaJseBond(Date maturityDate, double notional,  double annualCouponRate, 
            int couponMonth1, int couponDay1, int couponMonth2, int couponDay2, Calendar zaCalendar)        
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

            cfs = new List<Cashflow>();
            cfs.Add(new Cashflow(maturityDate, notional, Currency.ZAR));
            Init();
        }

        public override List<Cashflow> GetCFs()
        {
            return cfs;
        }

        public ResultStore GetResultStore()
        {
            ResultStore result = new ResultStore();
            result.Add("maturityDate", maturityDate);
            result.Add("annualCouponRate", annualCouponRate);
            return result;
        }
    }
}
