using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Core.Dates;


namespace QuantSA.Core.Products.Rates
{
    public class FloatLegASW : Product
    {
        protected double[] accrualFractions;
        protected Currency ccy;
        protected FloatRateIndex[] floatingIndices;
        protected double[] indexValues;
        protected double[] notionals;

        protected Date[] paymentDates;
        protected Date[] resetDates;
        protected double[] spreads;
        protected double payFixed;

        [JsonIgnore] protected Date valueDate;

        protected FloatLegASW()
        {
        }

        public FloatLegASW(double payFixed, Currency ccy, IEnumerable<Date> paymentDates, IEnumerable<double> notionals, IEnumerable<Date> resetDates,
            IEnumerable<FloatRateIndex> floatingIndices,
            IEnumerable<double> spreads, IEnumerable<double> accrualFractions)
        {
            this.payFixed = payFixed;
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
                    var floatingAmount = -payFixed * notionals[i] * accrualFractions[i] * (indexValues[i] + spreads[i]);
                    cfs.Add(new Cashflow(paymentDates[i], floatingAmount, ccy));
                }

            return cfs;
        }

        public override List<Currency> GetCashflowCurrencies()
        {
            return new List<Currency> { ccy };
        }

        public override List<Date> GetCashflowDates(Currency ccy)
        {
            var dates = new List<Date>();
            for (var i = 0; i < paymentDates.Length; i++)
                if (paymentDates[i] > valueDate)
                    dates.Add(paymentDates[i]);
            return dates;
        }

        public static FloatLegASW CreateFloatLegASW(double payFixed, Date calibrationDate, Date maturityDate, Tenor tenor, FloatRateIndex index,
        double spread, Calendar calendar)
        {
            DateGenerators.CreateDatesASWfloat(calibrationDate, maturityDate, tenor, index.Tenor, out var resetDates,
                out var paymentDates, out var accrualFractions, calendar);
            var notionals = resetDates.Select(d => 1e2);
            var floatingIndices = resetDates.Select(d => index);
            var spreads = resetDates.Select(d => spread);
            return new FloatLegASW(payFixed, index.Currency, paymentDates, notionals, resetDates, floatingIndices, spreads, accrualFractions);
        }
    }
}
