using System;
using System.Collections.Generic;
using System.Linq;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.General
{
    /// <summary>
    /// A par type credit default swap whose cashflows depend explicitly on the default events.  
    /// Protection always applies from the value date: there is not concept in the class of a forward starting 
    /// CDS.
    /// </summary>
    /// <seealso cref="QuantSA.General.Product" />
    public class CDS : Product
    {
        private readonly double[] accrualFractions;
        private readonly Currency ccy;

        /// <summary>
        /// 1 when we have sold protection.  -1 when we have bought protection.
        /// </summary>
        private readonly double cfMultiplier;

        // Market observables
        private readonly DefaultRecovery defaultRecovery;
        private readonly DefaultTime defaultTime;

        private readonly double[] notionals;

        // Contract definition
        private readonly Date[] paymentDates;
        private readonly double[] rates;
        private Date defaultTimeValue;
        private double recoveryRate;

        // Simulation values        
        private Date valueDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CDS"/> class.
        /// </summary>
        /// <param name="refEntity">The reference entity whose default is covered by this CDS.</param>
        /// <param name="ccy">The currency of the cashflows of the premium and default legs.</param>
        /// <param name="paymentDates">The payment dates on which the premium is paid.</param>
        /// <param name="notionals">The notionals that define the protection amount in the period until each payment date and the basis on which the premiums are calculated.</param>
        /// <param name="rates">The simple rates that apply until the default time.  Used to calculate the premium flows.</param>
        /// <param name="accrualFractions">The accrual fractions used to calculate the premiums paid on the <paramref name="paymentDates"/>.</param>
        /// <param name="boughtProtection">If set to <c>true</c> then protection has been bought and the premium will be paid.</param>
        public CDS(ReferenceEntity refEntity, Currency ccy, Date[] paymentDates, double[] notionals,
            double[] rates, double[] accrualFractions, bool boughtProtection)
        {
            defaultRecovery = new DefaultRecovery(refEntity);
            defaultTime = new DefaultTime(refEntity);
            this.ccy = ccy;
            this.paymentDates = paymentDates;
            this.notionals = notionals;
            this.rates = rates;
            this.accrualFractions = accrualFractions;
            cfMultiplier = boughtProtection ? -1.0 : 1.0;
        }

        /// <summary>
        /// Call this after <see cref="SetIndexValues(MarketObservable, double[])" /> to get all the cashflows on
        /// or AFTER the value date.
        /// </summary>
        /// <returns>
        /// A List of cashflows.  Under some circumstances it may be faster if these are ordered by
        /// increasing time.
        /// </returns>
        public override List<Cashflow> GetCFs()
        {
            var cfs = new List<Cashflow>();
            var previousDate = new Date(valueDate);
            for (var i = 0; i < paymentDates.Length; i++)
                if (paymentDates[i] > valueDate)
                {
                    if (paymentDates[i] < defaultTimeValue)
                    {
                        cfs.Add(new Cashflow(paymentDates[i],
                            cfMultiplier * notionals[i] * accrualFractions[i] * rates[i], ccy));
                    }
                    else
                    {
                        cfs.Add(new Cashflow(paymentDates[i], -cfMultiplier * notionals[i] * (1 - recoveryRate), ccy));
                        break;
                    }
                }

            return cfs;
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            // Going to trust that this will not be called for an irrelevant index.
            return new List<Date> {paymentDates.Last()};
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable> {defaultTime, defaultRecovery};
        }

        public override void Reset()
        {
            defaultTimeValue = null;
            recoveryRate = double.NaN;
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            if (index == defaultTime)
                defaultTimeValue = new Date(indexValues[0]);
            else if (index == defaultRecovery)
                recoveryRate = indexValues[0];
            else
                throw new ArgumentException("Unknown index: " + index);
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
                if (paymentDates[i] > valueDate && (defaultTimeValue == null || paymentDates[i] < defaultTimeValue))
                    dates.Add(paymentDates[i]);
            if (defaultTimeValue != null && defaultTimeValue <= paymentDates.Last())
                dates.Add(defaultTimeValue);
            return dates;
        }
    }
}