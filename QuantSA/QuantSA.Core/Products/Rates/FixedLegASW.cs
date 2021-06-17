using System.Collections.Generic;
using System.Linq;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Core.Dates;

namespace QuantSA.Core.Products.Rates
{
    public class FixedLegASW : Product
    {                                                   //removed accrualFractions
        private readonly Currency _ccy;
        private readonly double[] _notionals;
        private readonly Date[] _paymentDates;

        /// <summary>
        /// The simple rates used to calculate cashflows.
        /// </summary>
        private readonly double[] _rates;
        private readonly double _payFixed;

        private Date _valueDate;

        public FixedLegASW()
        {
        }

        public FixedLegASW(double payFixed, Currency ccy, IEnumerable<Date> paymentDates, IEnumerable<double> notionals, //removed accrualFractions and replaced it with 0.5 in the CFs calculation
            IEnumerable<double> rates)
        {
            _payFixed = payFixed;
            _ccy = ccy;
            _paymentDates = paymentDates.ToArray();
            _notionals = notionals.ToArray();
            _rates = rates.ToArray();
        }

        public override List<Cashflow> GetCFs()
        {
            var cfs = new List<Cashflow>();
            for (var i = 0; i < _paymentDates.Length; i++)
                if (_paymentDates[i] > _valueDate)
                    cfs.Add(new Cashflow(_paymentDates[i], _payFixed * _notionals[i] * 0.5 * _rates[i], _ccy));
            return cfs;
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            return new List<Date>();
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable>();
        }

        public override void Reset()
        {
            // Nothing do do.  Product has no state.
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            // Nothing to do. Product has no state.
        }

        public override void SetValueDate(Date valueDate)
        {
            _valueDate = valueDate;
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> { _ccy };
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            var dates = new List<Date>();
            for (var i = 0; i < _paymentDates.Length; i++)
                if (_paymentDates[i] > _valueDate)
                    dates.Add(_paymentDates[i]);
            return dates;
        }

        public static FixedLegASW CreateFixedLegASW(double payFixed, Date calibrationDate, Date maturityDate, Tenor tenor, FloatRateIndex index,
       double fixedRate, Calendar calendar)
        {
            DateGenerators.CreateDatesASWFixed(calibrationDate, maturityDate, tenor, index.Tenor, out var resetDates,
                out var paymentDates, calendar);
            var notionals = resetDates.Select(d => 1e2);
            var rates = resetDates.Select(d => fixedRate);
            return new FixedLegASW(payFixed, index.Currency, paymentDates, notionals, rates);
        }
    }
}
