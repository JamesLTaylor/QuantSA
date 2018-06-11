using System;
using System.Collections.Generic;
using QuantSA.Primitives.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.General
{
    [Serializable]
    public class FloatLeg : Product
    {
        protected double[] accrualFractions;
        protected Currency ccy;
        protected MarketObservable[] floatingIndices;
        protected double[] indexValues;
        protected double[] notionals;

        protected Date[] paymentDates;
        protected Date[] resetDates;
        protected double[] spreads;

        protected Date valueDate;

        protected FloatLeg()
        {
        }

        public FloatLeg(Currency ccy, Date[] paymentDates, double[] notionals, Date[] resetDates,
            FloatRateIndex[] floatingIndices,
            double[] spreads, double[] accrualFractions)
        {
            this.ccy = ccy;
            this.paymentDates = paymentDates;
            this.notionals = notionals;
            this.resetDates = resetDates;
            this.floatingIndices = floatingIndices;
            this.spreads = spreads;
            this.accrualFractions = accrualFractions;
        }


        public override void SetValueDate(Date valueDate)
        {
            this.valueDate = valueDate;
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