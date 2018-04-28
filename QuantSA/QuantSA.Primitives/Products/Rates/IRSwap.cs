using System;
using System.Collections.Generic;
using QuantSA.Primitives.Dates;

namespace QuantSA.General
{
    [Serializable]
    public class IRSwap : Product, IProvidesResultStore
    {
        private double[] accrualFractions;
        private Currency ccy;
        private double fixedRate;
        private MarketObservable index;
        private Date[] indexDates;
        private double[] indexValues;

        private double[] notionals;

        // Product specs
        private double payFixed; // -1 for payFixed, 1 for receive fixed
        private Date[] paymentDates;

        private double[] spreads;

        // Product state
        private Date valueDate;


        public IRSwap()
        {
        }

        /// <summary>
        /// Explicit constructor for IRSwap.  When possible use one of the static constructors.
        /// </summary>
        /// <param name="payFixed"></param>
        /// <param name="indexDates"></param>
        /// <param name="payDates"></param>
        /// <param name="index"></param>
        /// <param name="spreads"></param>
        /// <param name="accrualFractions"></param>
        /// <param name="notionals"></param>
        /// <param name="fixedRate"></param>
        /// <param name="ccy"></param>
        public IRSwap(double payFixed, Date[] indexDates, Date[] payDates, MarketObservable index, double[] spreads,
            double[] accrualFractions,
            double[] notionals, double fixedRate, Currency ccy)
        {
            this.payFixed = payFixed;
            this.indexDates = indexDates;
            paymentDates = payDates;
            this.index = index;
            this.spreads = spreads;
            this.accrualFractions = accrualFractions;
            this.notionals = notionals;
            this.fixedRate = fixedRate;
            this.ccy = ccy;

            indexValues = new double[indexDates.Length];
        }

        public ResultStore GetResultStore()
        {
            var swapDetails = new ResultStore();
            swapDetails.Add("payFixed", payFixed);
            swapDetails.Add("indexDates", indexDates);
            swapDetails.Add("payDates", paymentDates);
            swapDetails.Add("index", index.ToString());
            swapDetails.Add("spreads", spreads);
            swapDetails.Add("accrualFractions", accrualFractions);
            swapDetails.Add("notionals", notionals);
            swapDetails.Add("fixedRate", fixedRate);

            return swapDetails;
        }

        /// <summary>
        /// Constructor for ZAR market standard, fixed for float 3m Jibar swap.
        /// </summary>
        /// <param name="rate">The fixed rate paid or received</param>
        /// <param name="payFixed">Is the fixed rate paid?</param>
        /// <param name="notional">Flat notional for all dates.</param>
        /// <param name="startDate">First reset date of swap</param>
        /// <param name="tenor">Tenor of swap, must be a whole number of years.</param>
        /// <returns></returns>
        public static IRSwap CreateZARSwap(double rate, bool payFixed, double notional, Date startDate, Tenor tenor)
        {
            var newSwap = new IRSwap();
            var quarters = tenor.years * 4 + tenor.months / 3;
            newSwap.payFixed = payFixed ? -1 : 1;
            newSwap.indexDates = new Date[quarters];
            newSwap.paymentDates = new Date[quarters];
            newSwap.index = FloatingIndex.JIBAR3M;
            newSwap.spreads = new double[quarters];
            ;
            newSwap.accrualFractions = new double[quarters];
            ;
            newSwap.notionals = new double[quarters];
            newSwap.fixedRate = rate;
            newSwap.ccy = Currency.ZAR;
            newSwap.indexValues = new double[quarters];

            var date1 = new Date(startDate);
            Date date2;

            for (var i = 0; i < quarters; i++)
            {
                date2 = startDate.AddMonths(3 * (i + 1));
                newSwap.indexDates[i] = new Date(date1);
                newSwap.paymentDates[i] = new Date(date2);
                newSwap.spreads[i] = 0.0;
                newSwap.accrualFractions[i] = (date2 - date1) / 365.0;
                newSwap.notionals[i] = notional;
                date1 = new Date(date2);
            }

            return newSwap;
        }

        /// <summary>
        /// Returns the single floating rate index underlying this swap.
        /// </summary>
        /// <returns></returns>
        public FloatingIndex GetFloatingIndex()
        {
            return (FloatingIndex) index;
        }


        /// <summary>
        /// Set the date after which all cashflows will be required.
        /// </summary>
        /// <param name="valueDate"></param>
        public override void SetValueDate(Date valueDate)
        {
            // TODO: At this point all the dates in the future can be found to save some later looping.
            this.valueDate = valueDate;
        }

        /// <summary>
        /// 
        /// </summary>
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
            return new List<MarketObservable> {index};
        }

        /// <summary>
        /// The floating rate fixing dates that correspond to payment dates strctly after the value date.
        /// </summary>
        /// <param name="index">Will be the same index as returned by <see cref="GetRequiredIndices"/>.</param>
        /// <returns></returns>
        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            var requiredDates = new List<Date>();
            for (var i = 0; i < paymentDates.Length; i++)
                if (paymentDates[i] > valueDate)
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
            for (var i = 0; i < paymentDates.Length; i++)
                if (paymentDates[i] > valueDate)
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
            for (var i = 0; i < paymentDates.Length; i++)
                if (paymentDates[i] > valueDate)
                {
                    var fixedAmount = payFixed * notionals[i] * accrualFractions[i] * fixedRate;
                    var floatingAmount = -payFixed * notionals[i] * accrualFractions[i] * (indexValues[i] + spreads[i]);
                    cfs.Add(new Cashflow(paymentDates[i], fixedAmount, ccy));
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