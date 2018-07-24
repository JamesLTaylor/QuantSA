using System.Collections.Generic;
using Newtonsoft.Json;
using QuantSA.Core.Primitives;
using QuantSA.General;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Rates
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
    /// <seealso cref="IProductWithEarlyExercise" />
    public class BermudanSwaption : ProductWithEarlyExercise
    {
        private readonly List<Date> _exDates;
        private readonly bool _longOptionality;
        private readonly Product _postExerciseSwap;

        [JsonIgnore] private Date _valueDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="BermudanSwaption" /> class.
        /// </summary>
        /// <param name="postExerciseSwap">The post exercise swap.</param>
        /// <param name="exDates">The ex dates.</param>
        /// <param name="longOptionality">if set to <c>true</c> then the holder of this owns the optionality.</param>
        public BermudanSwaption(Product postExerciseSwap, List<Date> exDates, bool longOptionality)
        {
            _postExerciseSwap = postExerciseSwap;
            _exDates = exDates;
            _longOptionality = longOptionality;
        }

        /// <summary>
        /// Gets the products that this product will exercise into.  A Bermudan swaption exercised into a single swap.
        /// </summary>
        /// <remarks>
        /// It is a list in case the underlying product is different at each exercise date
        /// </remarks>
        /// <returns></returns>
        public override List<IProduct> GetPostExProducts()
        {
            return new List<IProduct> {_postExerciseSwap};
        }

        /// <summary>
        /// Gets the exercise dates of the option
        /// </summary>
        /// <returns></returns>
        public override List<Date> GetExerciseDates()
        {
            return _exDates;
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
        /// Is this product long optionality.  i.e. at each exercise date will the decision made by the holder in which case
        /// the value will be the maximum of the continuation or exercise value.
        /// </summary>
        /// <param name="exDate">The exercise date.  Must be in the list of dates returned by <see cref="GetExerciseDates" />.</param>
        /// <returns></returns>
        public override bool IsLongOptionality(Date exDate)
        {
            return _longOptionality;
        }

        /// <summary>
        /// Set the value date of the contract.
        /// </summary>
        /// <param name="valueDate"></param>
        public override void SetValueDate(Date valueDate)
        {
            _valueDate = valueDate;
        }

        public override void Reset()
        {
            // nothing to reset
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return _postExerciseSwap.GetCashflowCurrencies();
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return _postExerciseSwap.GetRequiredIndices();
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