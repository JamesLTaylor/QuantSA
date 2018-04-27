using System;
using System.Collections.Generic;
using QuantSA.Primitives.Conventions.BusinessDay;
using QuantSA.Primitives.Conventions.DayCount;
using QuantSA.Primitives.Dates;
using QuantSA.Primitives.MarketObservables;

namespace QuantSA.Primitives.Products.Rates
{
    /// <summary>
    /// A FRA that pays the discounted flow at the near date/reset date.
    /// </summary>
    /// <seealso cref="ProductWrapper" />
    [Serializable]
    public class FRA : ProductWrapper, IProvidesResultStore
    {
        private double notional;
        private double accrualFraction;
        private double rate;
        private Date nearDate;
        private Date farDate;
        private FloatingIndex floatIndex;
        private bool payFixed;
        private Currency ccy;

        /// <summary>
        /// Creates a FRA according to South African conventions.
        /// </summary>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="notional">The notional used in calculating the cashflow.</param>
        /// <param name="rate">The fixed rate paid or received on the fra.</param>
        /// <param name="fraCode">The fra code, eg '3x6'.</param>
        /// <param name="payFixed">if set to <c>true</c> the the fixed rate is paid..</param>
        /// <param name="zaCalendar">The za calendar.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        public static FRA CreateZARFra(Date tradeDate, double notional, double rate, string fraCode, bool payFixed, Calendar zaCalendar)
        {            
            string[] parts = fraCode.ToLower().Trim().Split('x');
            if (parts.Length != 2) throw new ArgumentException(fraCode + " is not of the required form.  FRA code must be of the form 'mxn' for integer m and n, example: '3x6'");
            int near = 0;
            int far = 0;
            if (!int.TryParse(parts[0], out near)) throw new ArgumentException(fraCode + " is not of the required form.  FRA code must be of the form 'mxn' for integer m and n, example: '3x6'");
            if (!int.TryParse(parts[1], out far)) throw new ArgumentException(fraCode + " is not of the required form.  FRA code must be of the form 'mxn' for integer m and n, example: '3x6'");
            if ((far - near) != 3) throw new ArgumentException(fraCode + " is not of the required form.  The near and far number of months must differ by 3.");
            var mf = BusinessDayStore.ModifiedFollowing;
            Date nearDate = mf.Adjust(tradeDate.AddMonths(near), zaCalendar);
            Date farDate = mf.Adjust(tradeDate.AddMonths(far), zaCalendar);
            double accrualFraction = DayCountStore.Actual365Fixed.YearFraction(nearDate, farDate);
            return new FRA(notional, accrualFraction, rate, payFixed, nearDate, farDate, FloatingIndex.JIBAR3M);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FRA"/> class.
        /// </summary>
        /// <param name="notional">The notional.</param>
        /// <param name="accrualFraction">The accrual fraction.  Used for calculating the cashflow and 
        /// discounting it FRAs that pay on the reset date.</param>
        /// <param name="nearDate">The near date.</param>
        /// <param name="farDate">The far date.</param>
        /// <param name="floatIndex">The floating rate index that will referenced by the FRA.  Will also 
        /// determine the currency of the cashflow.</param>
        public FRA(double notional, double accrualFraction, double rate, bool payFixed, Date nearDate, 
            Date farDate, FloatingIndex floatIndex)
        {
            this.accrualFraction = accrualFraction;
            this.notional = notional;
            this.rate = rate;
            this.payFixed = payFixed;
            this.nearDate = nearDate;
            this.farDate = farDate;
            this.floatIndex = floatIndex;
            ccy = floatIndex.currency;
            Init();
        }

        public override List<Cashflow> GetCFs()
        {
            double indexValue = Get(floatIndex, nearDate);
            double amount = notional * (indexValue - rate) * accrualFraction / (1 + indexValue * accrualFraction);
            if (!payFixed) amount = -1.0 * amount;
            return new List<Cashflow> { new Cashflow(nearDate, amount, ccy) };
        }

        public ResultStore GetResultStore()
        {
            ResultStore result = new ResultStore();
            result.Add("notional", notional);
            result.Add("accrualFraction", accrualFraction);
            result.Add("rate", rate);
            result.Add("nearDate", nearDate);
            result.Add("farDate", farDate);
            result.Add("floatIndex", floatIndex.ToString());
            result.Add("payFixed", payFixed.ToString());
            result.Add("ccy", ccy.ToString());
            return result;
        }
    }
}
