using System.Collections.Generic;
using Newtonsoft.Json;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;


namespace QuantSA.Core.Products.Rates
{
    public class InflationLinkedSwap : Product
    {
        public double payFixed; // -1 for payFixed, 1 for receive fixed
        public Date startDate;
        public double nominal;
        public Tenor tenor;
        public double fixedRate;
        public FloatRateIndex index;
        public double spread;
        public Date[] indexDates;
        public Date[] paymentDatesFloating;
        public double[] accrualFractions;
        public Calendar zaCalendar;
        public Currency ccy;

        // Product state
        [JsonIgnore] private double[] indexValues;
        [JsonIgnore] private Date _valueDate;

        public InflationLinkedSwap(double _payFixed, Date _startDate, double _nominal, Tenor _tenor, double _fixedRate, FloatRateIndex _index, Date[] _indexDates, Date[] _payDatesFloating,
            double _spread, double[] _accrualFractions, Calendar _zaCalendar, Currency _ccy)
        {
            payFixed = _payFixed;
            startDate = _startDate;
            nominal = _nominal;
            tenor = _tenor;
            fixedRate = _fixedRate;
            index = _index;
            indexDates = _indexDates;
            paymentDatesFloating = _payDatesFloating;
            spread = _spread;
            accrualFractions = _accrualFractions;
            zaCalendar = _zaCalendar;
            ccy = _ccy;
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
            indexValues = new double[indexDates.Length];

        }

        /// <summary>
        public override void Reset()
        {
            indexValues = new double[indexDates.Length];
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
            for (var i = 0; i < paymentDatesFloating.Length; i++)
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
            for (var i = 0; i < paymentDatesFloating.Length; i++)
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

            for (var i = 0; i < paymentDatesFloating.Length; i++)
                if ((paymentDatesFloating[i] > _valueDate))
                {
                    var floatingAmount = -payFixed * nominal * (1 + (indexValues[i] + spread) * accrualFractions[i]);
                    nominal = floatingAmount;
                    cfs.Add(new Cashflow(paymentDatesFloating[i], floatingAmount, ccy));
                }

            return cfs;
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            var dates = new List<Date>();
            for (var i = 0; i < paymentDatesFloating.Length; i++)
                if (paymentDatesFloating[i] > _valueDate)
                    dates.Add(paymentDatesFloating[i]);

            return dates;
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> { ccy };
        }

    }

}
