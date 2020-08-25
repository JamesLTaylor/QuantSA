using System.Collections.Generic;
using System.Linq;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Rates
{
    public class FixedLeg : Product
    {
        private readonly double[] _accrualFractions;
        private readonly Currency _ccy;
        private readonly double[] _notionals;
        private readonly Date[] _paymentDates;

        /// <summary>
        /// The simple rates used to calculate cashflows.
        /// </summary>
        private readonly double[] _rates;

        private Date _valueDate;

        public FixedLeg()
        {
        }

        public FixedLeg(Currency ccy, IEnumerable<Date> paymentDates, IEnumerable<double> notionals,
            IEnumerable<double> rates,
            IEnumerable<double> accrualFractions)
        {
            _ccy = ccy;
            _paymentDates = paymentDates.ToArray();
            _notionals = notionals.ToArray();
            _rates = rates.ToArray();
            _accrualFractions = accrualFractions.ToArray();
        }

        public override List<Cashflow> GetCFs()
        {
            var cfs = new List<Cashflow>();
            for (var i = 0; i < _paymentDates.Length; i++)
                if (_paymentDates[i] > _valueDate)
                    cfs.Add(new Cashflow(_paymentDates[i], _notionals[i] * _accrualFractions[i] * _rates[i], _ccy));
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
            return new List<Currency> {_ccy};
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            var dates = new List<Date>();
            for (var i = 0; i < _paymentDates.Length; i++)
                if (_paymentDates[i] > _valueDate)
                    dates.Add(_paymentDates[i]);
            return dates;
        }
    }
}