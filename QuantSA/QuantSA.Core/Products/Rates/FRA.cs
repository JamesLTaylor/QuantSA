using System;
using System.Collections.Generic;
using QuantSA.Shared.Conventions.BusinessDay;
using QuantSA.Shared.Conventions.DayCount;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Rates
{
    /// <summary>
    /// A FRA that pays the discounted flow at the near date/reset date.
    /// </summary>
    /// <seealso cref="ProductWrapper" />
    public class FRA : ProductWrapper
    {
        private readonly double _accrualFraction;
        private readonly Currency _ccy;
        private readonly Date _farDate;
        private readonly FloatRateIndex _floatIndex;
        private readonly Date _nearDate;
        private readonly double _notional;
        private readonly bool _payFixed;
        private readonly double _rate;

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
            Date farDate, FloatRateIndex floatIndex)
        {
            _accrualFraction = accrualFraction;
            _notional = notional;
            _rate = rate;
            _payFixed = payFixed;
            _nearDate = nearDate;
            _farDate = farDate;
            _floatIndex = floatIndex;
            _ccy = floatIndex.Currency;
            Init();
        }

        /// <summary>
        /// Creates a FRA according to South African conventions.
        /// </summary>
        /// <param name="tradeDate">The trade date.</param>
        /// <param name="notional">The notional used in calculating the cashflow.</param>
        /// <param name="rate">The fixed rate paid or received on the fra.</param>
        /// <param name="fraCode">The fra code, eg '3x6'.</param>
        /// <param name="payFixed">if set to <c>true</c> the fixed rate is paid..</param>
        /// <param name="zaCalendar">The za calendar.</param>
        /// <param name="jibar"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        public static FRA CreateZARFra(Date tradeDate, double notional, double rate, string fraCode, bool payFixed,
            Calendar zaCalendar, FloatRateIndex jibar)
        {
            var parts = fraCode.ToLower().Trim().Split('x');
            if (parts.Length != 2)
                throw new ArgumentException(
                    fraCode +
                    " is not of the required form.  FRA code must be of the form 'mxn' for integer m and n, example: '3x6'");
            var near = 0;
            var far = 0;
            if (!int.TryParse(parts[0], out near))
                throw new ArgumentException(
                    fraCode +
                    " is not of the required form.  FRA code must be of the form 'mxn' for integer m and n, example: '3x6'");
            if (!int.TryParse(parts[1], out far))
                throw new ArgumentException(
                    fraCode +
                    " is not of the required form.  FRA code must be of the form 'mxn' for integer m and n, example: '3x6'");
            if (far - near != 3)
                throw new ArgumentException(
                    fraCode + " is not of the required form.  The near and far number of months must differ by 3.");
            var mf = BusinessDayStore.ModifiedFollowing;
            var nearDate = mf.Adjust(tradeDate.AddMonths(near), zaCalendar);
            var farDate = mf.Adjust(tradeDate.AddMonths(far), zaCalendar);
            var accrualFraction = DayCountStore.Actual365Fixed.YearFraction(nearDate, farDate);
            return new FRA(notional, accrualFraction, rate, payFixed, nearDate, farDate, jibar);
        }

        public override List<Cashflow> GetCFs()
        {
            var indexValue = Get(_floatIndex, _nearDate);
            var amount = _notional * (indexValue - _rate) * _accrualFraction / (1 + indexValue * _accrualFraction);
            if (!_payFixed) amount = -1.0 * amount;
            return new List<Cashflow> {new Cashflow(_nearDate, amount, _ccy)};
        }
    }
}