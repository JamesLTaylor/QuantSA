using System.Collections.Generic;
using System.Linq;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.General
{
    public class FixedLeg : Product
    {
        private readonly double[] accrualFractions;
        private readonly Currency ccy;
        private readonly double[] notionals;
        private readonly Date[] paymentDates;

        /// <summary>
        /// The simple rates used to calculate cashflows.
        /// </summary>
        private readonly double[] rates;

        private Date valueDate;

        public FixedLeg()
        {
        }

        public FixedLeg(Currency ccy, IEnumerable<Date> paymentDates, IEnumerable<double> notionals,
            IEnumerable<double> rates,
            IEnumerable<double> accrualFractions)
        {
            this.ccy = ccy;
            this.paymentDates = paymentDates.ToArray();
            this.notionals = notionals.ToArray();
            this.rates = rates.ToArray();
            this.accrualFractions = accrualFractions.ToArray();
        }

        public override List<Cashflow> GetCFs()
        {
            var cfs = new List<Cashflow>();
            for (var i = 0; i < paymentDates.Length; i++)
                if (paymentDates[i] > valueDate)
                    cfs.Add(new Cashflow(paymentDates[i], notionals[i] * accrualFractions[i] * rates[i], ccy));
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
            this.valueDate = valueDate;
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> {ccy};
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            var dates = new List<Date>();
            for (var i = 0; i < paymentDates.Length; i++)
                if (paymentDates[i] > valueDate)
                    dates.Add(paymentDates[i]);
            return dates;
        }
    }
}