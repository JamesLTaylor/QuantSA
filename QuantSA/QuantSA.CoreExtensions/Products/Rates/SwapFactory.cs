using System.Linq;
using QuantSA.Core.Dates;
using QuantSA.Core.Products.Rates;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.CoreExtensions.Products.Rates
{
    public static class SwapFactory
    {
        /// <summary>
        /// Constructor for ZAR market standard, fixed for float 3m Jibar swap.
        /// </summary>
        /// <param name="rate">The fixed rate paid or received</param>
        /// <param name="payFixed">Is the fixed rate paid?</param>
        /// <param name="notional">Flat notional for all dates.</param>
        /// <param name="startDate">First reset date of swap</param>
        /// <param name="tenor">Tenor of swap, must be a whole number of years.</param>
        /// <param name="floatRateIndex"></param>
        /// <returns></returns>
        public static IRSwap CreateZARSwap(double rate, bool payFixed, double notional, Date startDate, Tenor tenor,
            FloatRateIndex floatRateIndex)
        {
            var quarters = tenor.Years * 4 + tenor.Months / 3;
            var indexDates = new Date[quarters];
            var paymentDates = new Date[quarters];
            var spreads = new double[quarters];
            var accrualFractions = new double[quarters];
            var notionals = new double[quarters];
            var fixedRate = rate;

            var date1 = new Date(startDate);

            for (var i = 0; i < quarters; i++)
            {
                var date2 = startDate.AddMonths(3 * (i + 1));
                indexDates[i] = new Date(date1);
                paymentDates[i] = new Date(date2);
                spreads[i] = 0.0;
                accrualFractions[i] = (date2 - date1) / 365.0;
                notionals[i] = notional;
                date1 = new Date(date2);
            }

            var newSwap = new IRSwap(payFixed ? -1 : 1, indexDates, paymentDates, floatRateIndex, spreads,
                accrualFractions,
                notionals, fixedRate, floatRateIndex.Currency);
            return newSwap;
        }

        /// <summary>
        /// Creates Bermudan swaption with a simple ZAR swap as underlying, the ZAR swap is the same as that created by:
        ///  <see cref="CreateZARSwap"/>.
        /// </summary>
        /// <param name="exerciseDates">The exercise dates.  The dates on which the person who is long optionality can exercise.</param>
        /// <param name="longOptionality">if set to <c>true</c> then the person valuing this product owns the optionality.</param>
        /// <param name="rate">The fixed rate on the underlying swap.</param>
        /// <param name="payFixed">if set to <c>true</c> then the underlying swap has the person valuing the product paying fixed after exercise.</param>
        /// <param name="notional">The constant notional in ZAR on the underlying swap.</param>
        /// <param name="startDate">The start date of the underlying swap.</param>
        /// <param name="tenor">The tenor of the underlying swap.</param>
        /// <param name="floatRateIndex"></param>
        /// <returns></returns>
        public static BermudanSwaption CreateZARBermudanSwaption(Date[] exerciseDates, bool longOptionality,
            double rate,
            bool payFixed, double notional, Date startDate, Tenor tenor, FloatRateIndex floatRateIndex)
        {
            var swap = CreateZARSwap(rate, payFixed, notional, startDate, tenor, floatRateIndex);
            var swaption = new BermudanSwaption(swap, exerciseDates.ToList(), longOptionality);
            return swaption;
        }

        /// <summary>
        /// Create a <see cref="FloatLeg"/>.
        /// </summary>
        /// <param name="calibrationDate"></param>
        /// <param name="tenor"></param>
        /// <param name="index"></param>
        /// <param name="spread"></param>
        /// <returns></returns>
        public static FloatLeg CreateFloatLeg(Date calibrationDate, Tenor tenor, FloatRateIndex index,
            double spread)
        {
            DateGenerators.CreateDatesNoHolidays(calibrationDate, tenor, index.Tenor, out var resetDates,
                out var paymentDates, out var accrualFractions);
            var notionals = resetDates.Select(d => 1e6);
            var floatingIndices = resetDates.Select(d => index);
            var spreads = resetDates.Select(d => spread);
            return new FloatLeg(index.Currency, paymentDates, notionals, resetDates, floatingIndices, spreads,
                accrualFractions);
        }

        /// <summary>
        /// Create a <see cref="FloatLeg"/>.
        /// </summary>
        /// <param name="calibrationDate"></param>
        /// <param name="tenor"></param>
        /// <param name="index"></param>
        /// <param name="fixedRate"></param>
        /// <returns></returns>
        public static FixedLeg CreateFixedLeg(Date calibrationDate, Tenor tenor, FloatRateIndex index,
            double fixedRate)
        {
            DateGenerators.CreateDatesNoHolidays(calibrationDate, tenor, index.Tenor, out var resetDates,
                out var paymentDates, out var accrualFractions);
            var notionals = resetDates.Select(d => 1e6);
            var rates = resetDates.Select(d => fixedRate);
            return new FixedLeg(index.Currency, paymentDates, notionals, rates, accrualFractions);
        }
    }
}