using System;
using System.Collections.Generic;
using System.Linq;
using QuantSA.Core.Products.SAMarket;
using QuantSA.Shared;
using QuantSA.Shared.Dates;

namespace QuantSA.CoreExtensions.SAMarket
{
    public static class JSEBondForwardEx
    {
        // Get coupon dates for books close dates that lie between settlement date and forward date
        private static List<Date> GetCouponDates(this BesaJseBond bond, Date settleDate, Date forwardDate)
        {
            var BooksCloseDates = new List<Date> ();
            var CouponDates = new List<Date>();

            var yr = settleDate.Year;
            while (yr >= settleDate.Year && yr < forwardDate.Year + 1)
            {
                var BCD1 = (new Date(yr, bond.couponMonth1, bond.couponDay1).AddDays(-bond.booksCloseDateDays));
                var BCD2 = (new Date(yr, bond.couponMonth2, bond.couponDay2).AddDays(-bond.booksCloseDateDays));

                if (BCD1 > settleDate && BCD1 < forwardDate)
                {
                    CouponDates.Add(new Date(yr, bond.couponMonth1, bond.couponDay1));
                    BooksCloseDates.Add(BCD1);
                }

                if (BCD2 > settleDate && BCD2 < forwardDate)
                {
                    CouponDates.Add(new Date(yr, bond.couponMonth2, bond.couponDay2));
                    BooksCloseDates.Add(BCD2);
                }

                yr += 1;
            }

            return CouponDates;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bondforward"></param>
        /// <param name="settleDate"></param>
        /// <param name="ytm"></param>
        /// <param name="repo"></param>
        /// <returns></returns>
        public static ResultStore ForwardPrice(this JSEBondForward bondforward, Date settleDate, double ytm, double repo)
        {
            var N = 100.0;
            var couponamount = N * bondforward.underlyingBond.annualCouponRate / 2;
            var forwardDate = bondforward.forwardDate;

            if (settleDate > forwardDate)
                throw new ArgumentException("settlement date must be before forward date.");

            // get all-in price of underlying bond
            var results = bondforward.underlyingBond.GetSpotMeasures(settleDate, ytm);
            var AIP = (double)results.GetScalar(BesaJseBondEx.Keys.RoundedAip);

            // calculate Unadjusted Forward Price
            var dt = (double)(forwardDate - settleDate) / 365;
            var ForwardPrice = AIP * (1 + repo * dt);

            // get coupon dates between settlement and forward date and calculate equivalent value function
            var CouponDates = GetCouponDates(bondforward.underlyingBond, settleDate, forwardDate);

            // adjust forward price for any coupons the counterparty might receive
            var EV = new List<double>();

            double AdjustedForwardPrice = 0;
            if (CouponDates.Any())
            {
                foreach (var date in CouponDates)
                {
                    if (date <= forwardDate)
                    {
                        EV.Add(1 + repo * (forwardDate - date) / 365);
                    }
                    else
                    {
                        EV.Add(Math.Pow(1 + repo * (date - forwardDate) / 365, -1));
                    }
                }

                AdjustedForwardPrice = ForwardPrice - couponamount * EV.Sum();
            }
            else
            {
                AdjustedForwardPrice = ForwardPrice;
            }

            var resultStore = new ResultStore();
            resultStore.Add(Keys.ForwardPrice, AdjustedForwardPrice);
            return resultStore;
        }

        public static class Keys
        {
            public const string ForwardPrice = "AdjustedForwardPrice";
        }
    }
}
