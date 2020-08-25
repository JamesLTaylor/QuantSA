using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Rates
{
    public class FloatLeg : Product
    {
        protected double[] accrualFractions;
        protected Currency ccy;
        protected FloatRateIndex[] floatingIndices;
        protected double[] indexValues;
        protected double[] notionals;

        protected Date[] paymentDates;
        protected Date[] resetDates;
        protected double[] spreads;

        [JsonIgnore] protected Date valueDate;

        protected FloatLeg()
        {
        }

        public FloatLeg(Currency ccy, IEnumerable<Date> paymentDates, IEnumerable<double> notionals, IEnumerable<Date> resetDates,
            IEnumerable<FloatRateIndex> floatingIndices,
            IEnumerable<double> spreads, IEnumerable<double> accrualFractions)
        {
            this.ccy = ccy;
            this.paymentDates = paymentDates.ToArray();
            this.notionals = notionals.ToArray();
            this.resetDates = resetDates.ToArray();
            this.floatingIndices = floatingIndices.ToArray();
            this.spreads = spreads.ToArray();
            this.accrualFractions = accrualFractions.ToArray();
        }


        public override void SetValueDate(Date valueDate)
        {
            this.valueDate = valueDate;
            indexValues = new double[resetDates.Length];
        }

        public override void Reset()
        {
            indexValues = new double[resetDates.Length];
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            var hashSet = new HashSet<MarketObservable>(floatingIndices);
            return new List<MarketObservable>(hashSet);
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            var requiredDates = new List<Date>();
            for (var i = 0; i < paymentDates.Length; i++)
                if (paymentDates[i] > valueDate && index.Equals(floatingIndices[i]))
                    requiredDates.Add(resetDates[i]);
            return requiredDates;
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            var indexCounter = 0;
            for (var i = 0; i < paymentDates.Length; i++)
                if (paymentDates[i] > valueDate && index.Equals(floatingIndices[i]))
                {
                    this.indexValues[i] = indexValues[indexCounter];
                    indexCounter++;
                }
        }

        public override List<Cashflow> GetCFs()
        {
            var cfs = new List<Cashflow>();
            for (var i = 0; i < paymentDates.Length; i++)
                if (paymentDates[i] > valueDate)
                {
                    var floatingAmount = notionals[i] * accrualFractions[i] * (indexValues[i] + spreads[i]);
                    cfs.Add(new Cashflow(paymentDates[i], floatingAmount, ccy));
                }

            return cfs;
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