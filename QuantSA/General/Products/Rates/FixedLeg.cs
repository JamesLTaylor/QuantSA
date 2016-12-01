using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    [Serializable]
    public class FixedLeg : Product
    {
        Date[] paymentDates;
        /// <summary>
        /// The simple rates used to calculate cashflows.
        /// </summary>
        double[] rates;
        double[] notionals;
        double[] accrualFractions;
        Currency ccy;

        Date valueDate;

        public FixedLeg() { }

        public FixedLeg(Currency ccy, Date[] paymentDates, double[] notionals, double[] rates, double[] accrualFractions)
        {
            this.ccy = ccy;
            this.paymentDates = paymentDates;
            this.notionals = notionals;
            this.rates = rates;
            this.accrualFractions = accrualFractions;
        }

        public override List<Cashflow> GetCFs()
        {
            List<Cashflow> cfs = new List<Cashflow>();
            for (int i = 0; i< paymentDates.Length; i++)
            {
                if (paymentDates[i] > valueDate) cfs.Add(new Cashflow(paymentDates[i], notionals[i] * accrualFractions[i] * rates[i], ccy));
            }
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
            return new List<Currency> { ccy };
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            List<Date> dates = new List<Date>();
            for (int i = 0; i < paymentDates.Length; i++)
            {
                if (paymentDates[i] > valueDate) dates.Add(paymentDates[i]);
            }
            return dates;
        }
    }
}
