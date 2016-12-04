using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// The cashflows on a product which exercises into another are of two type:
    /// <para/>
    /// 1: Cashflows that take place until exercise, we assume that cashflows that take 
    /// place before an exercise are independent of the time of the exercise
    /// <para/>
    /// 2: Cashflows that take place if exercise occurs at an exercise date.  For example 
    /// a penalty that must be paid at the point of exercise on a cancellable swap.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <seealso cref="QuantSA.General.ProductWithEarlyExercise" />
    [Serializable]
    public class BermudanSwaption : ProductWithEarlyExercise
    {
        List<Date> exDates;
        Product postExerciseSwap;
        Date valueDate;
        Currency ccy = Currency.ZAR;
        bool longOptionality;

        /// <summary>
        /// Creates Bermudan swaption with a simple ZAR swap as underlying, the ZAR swap is the same as that created by:
        ///  <see cref="IRSwap.CreateZARSwap"/>.
        /// </summary>
        /// <param name="exerciseDates">The exercise dates.  The dates on which the person who is long optionality can exercise.</param>
        /// <param name="longOptionality">if set to <c>true</c> then the person valuing this product owns the optionality.</param>
        /// <param name="rate">The fixed rate on the underlying swap.</param>
        /// <param name="payFixed">if set to <c>true</c> then the underlying swap has the person valuaing the product paying fixed after exercise.</param>
        /// <param name="notional">The constant notional in ZAR on the underlying swap.</param>
        /// <param name="startDate">The start date of the underlying swap.</param>
        /// <param name="tenor">The tenor of the underlying swap.</param>
        /// <returns></returns>
        public static BermudanSwaption CreateZARBermudanSwaption(Date[] exerciseDates, bool longOptionality, double rate, 
            bool payFixed, double notional, Date startDate, Tenor tenor)
        {
            IRSwap swap = IRSwap.CreateZARSwap(rate, payFixed, notional, startDate, tenor);
            BermudanSwaption swaption = new BermudanSwaption(swap, exerciseDates.ToList(), longOptionality);
            return swaption;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BermudanSwaption" /> class.
        /// </summary>
        /// <param name="postExerciseSwap">The post exercise swap.</param>
        /// <param name="exDates">The ex dates.</param>
        /// <param name="longOptionality">if set to <c>true</c> then the holder of this owns the optionality.</param>
        public BermudanSwaption(Product postExerciseSwap, List<Date> exDates, bool longOptionality)
        {
            this.postExerciseSwap = postExerciseSwap;
            this.exDates = exDates;            
            this.longOptionality = longOptionality;
        }

        /// <summary>
        /// Gets the products that this product will exercise into.  A Bermudan swaption exercised into a single swap.
        /// </summary>
        /// <remarks>
        /// It is a list in case the underlying product is different at each exercise date
        /// </remarks>
        /// <returns></returns>
        public override List<Product> GetPostExProducts()
        {
            return new List<Product> { postExerciseSwap };
        }

        /// <summary>
        /// Gets the exercise dates of the option
        /// </summary>
        /// <returns></returns>
        public override List<Date> GetExerciseDates()
        {
            return exDates;
        }

        /// <summary>
        /// Gets the product that will be exercised into at this date.  Returned as an index of the list of
        /// products in <see cref="GetPostExProducts"/>
        /// </summary>
        /// <param name="exDate">The exercise date.  Must be in the list of dates returned by <see cref="GetExerciseDates" />.</param>
        /// <returns></returns>
        public override int GetPostExProductAtDate(Date exDate)
        {
            return 0;
        }

        /// <summary>
        /// Is this product long optionality.  ie at each exercise date will the decision made by the holder in which case
        /// the value will be the maximum of the continuation or exercise value.
        /// </summary>
        /// <param name="exDate">The exercise date.  Must be in the list of dates returned by <see cref="GetExerciseDates" />.</param>
        /// <returns></returns>
        public override bool IsLongOptionality(Date exDate)
        {
            return longOptionality;
        }

        /// <summary>
        /// Set the value date of the contract.
        /// </summary>
        /// <param name="valueDate"></param>
        public override void SetValueDate(Date valueDate)
        {
            this.valueDate = valueDate;
        }

        public override void Reset()
        {
            // nothing to reset
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> { ccy };
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable>();
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            return new List<Date>();
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            return new List<Date>();
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            // Do nothing
        }

        public override List<Cashflow> GetCFs()
        {
            return new List<Cashflow>();
        }

    }
}
