using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Core.Products.SAMarket;


namespace QuantSA.Core.Products.Rates
{
    public class AssetSwap : Product
    {
        public double payFixed; // -1 for payFixed, 1 for receive fixed
        public List<Date> indexDates;
        public List<Date> paymentDatesFixed;
        public List<Date> paymentDatesFloating;
        public double spread;
        public FloatRateIndex index;
        public List<double> accrualFractions;
        public double fixedRate;
        public Date maturityDate;
        public int couponMonth1;
        public int couponDay1;
        public int couponMonth2;
        public int couponDay2;
        public int booksCloseDateDays;
        public Calendar zaCalendar;
        public Currency ccy;
        public double[] indexValues1;


        // Product state
        [JsonIgnore] private double[] indexValues;
        [JsonIgnore] private Date _valueDate;

        public AssetSwap(double _payFixed, double _fixedRate, FloatRateIndex _index,  List<Date> _indexDates, List<Date> _payDatesFloating, 
            List<Date> _payDatesFixed, double _spread, int _couponMonth1, int _couponDay1, int _couponMonth2, int _couponDay2, int _booksCloseDateDays,
            Date _maturityDate, List<double> _accrualFractions, Calendar _zaCalendar, Currency _ccy, double[] _indexValues1)
        {
            payFixed = _payFixed;
            fixedRate = _fixedRate;
            index = _index;
            indexDates = _indexDates;
            paymentDatesFloating = _payDatesFloating;
            paymentDatesFixed = _payDatesFixed;
            spread = _spread;
            couponMonth1 = _couponMonth1;
            couponDay1 = _couponDay1;
            couponMonth2 = _couponMonth2;
            couponDay2 = _couponDay2;
            booksCloseDateDays = _booksCloseDateDays;
            maturityDate = _maturityDate;
            accrualFractions = _accrualFractions;
            zaCalendar = _zaCalendar;
            ccy = _ccy;
            indexValues1 = _indexValues1;

        }

        /// <summary>
        /// Returns the single floating rate index underlying this swap.
        public FloatRateIndex GetFloatingIndex()
        {
            return index;
        }

        /// <summary>
        /// Set the date after which all cashflows will be required.
        /// </summary>
        /// <param name="valueDate"></param>
        public override void SetValueDate(Date valueDate)
        {
            _valueDate = valueDate;
            indexValues = new double [indexDates.Count];
            
        }

        /// <summary>
        public override void Reset()
        {
            indexValues = new double [indexDates.Count];
        }

        /// <summary>
        /// A swap only needs a single floating rate index.
        /// </summary>
        /// <returns></returns>
        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable> { index };

        }

        /// <summary>
        /// The floating rate fixing dates that correspond to payment dates strictly after the value date.
        /// </summary>
        /// <param name="index">Will be the same index as returned by <see cref="GetRequiredIndices"/>.</param>
        /// <returns></returns>
        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            var requiredDates = new List<Date>();
            for (var i = 0; i < paymentDatesFloating.Count; i++)
                if (paymentDatesFloating[i] > _valueDate)
                    requiredDates.Add(indexDates[i]);
            return requiredDates;
        }

        /// <summary>
        /// Sets the values of the floating rates at all reset dates corresponding to payment dates after the value dates.
        /// </summary>
        /// <param name="index">Must only be called for the single index underlying the floating rate of the swap.</param>
        /// <param name="indexValues">An array of values the same length as the dates returned in <see cref="GetRequiredIndexDates(MarketObservable)"/>.</param>
        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            var indexCounter = 0;
            for (var i = 0; i < paymentDatesFloating.Count; i++)
                if (paymentDatesFloating[i] > _valueDate)
                {
                    this.indexValues[i] = indexValues[indexCounter];
                    indexCounter++;
                }
        }

        /// <summary>
        /// The actual implementation of the contract cashflows.
        /// </summary>
        /// <returns></returns>
        public override List<Cashflow> GetCFs()
        {
            var cfs = new List<Cashflow>();
            for (var i = 0; i < paymentDatesFloating.Count; i++)
                if (paymentDatesFloating[i] > _valueDate)
                {
                    var floatingAmount = -payFixed * 100 * accrualFractions[i] * (indexValues.ElementAt(i) + spread);
                    cfs.Add(new Cashflow(paymentDatesFloating[i], floatingAmount, ccy));
                }

            for (var i = 0; i < paymentDatesFixed.Count; i++)
                if (paymentDatesFixed[i] > _valueDate)
                {
                    var fixedAmount = payFixed * 100 * 0.5 * fixedRate;
                    cfs.Add(new Cashflow(paymentDatesFixed[i], fixedAmount, ccy));
                }

            return cfs;
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            var dates = new List<Date>();
            for (var i = 0; i < paymentDatesFloating.Count; i++)
                if (paymentDatesFloating[i] > _valueDate)
                    dates.Add(paymentDatesFloating[i]);

            for (var i = 0; i < paymentDatesFixed.Count; i++)
                if (paymentDatesFixed[i] > _valueDate) 
                    dates.Add(paymentDatesFixed[i]);
            return dates;
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> { ccy };
        }

    }

}
