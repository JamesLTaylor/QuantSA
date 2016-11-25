using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantSA.General
{
    /// <summary>
    /// A credit default swap whose cashflows depend explicitly on the default events.
    /// </summary>
    /// <seealso cref="QuantSA.General.Product" />
    [Serializable]
    public class CDS : Product
    {
        // Contract definition
        Date[] paymentDates;
        double[] rates;
        double[] notionals;
        double[] accrualFractions;
        Currency ccy;
        /// <summary>
        /// 1 when we have sold protection.  -1 when we have bought protection.
        /// </summary>
        double cfMultiplier;

        // Market observables
        DefaultRecovery defaultRecovery;
        DefaultTime defaultTime;

        // Simulation values        
        Date valueDate;
        Date defaultTimeValue;
        double recoveryRate;

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

        public override List<Cashflow> GetCFs()
        {
            List<Cashflow> cfs = new List<Cashflow>();
            Date previousDate = new Date(valueDate);
            for (int i = 0; i < paymentDates.Length; i++)
            {
                if (paymentDates[i] > valueDate)
                {
                    if (paymentDates[i] < defaultTimeValue)
                    {
                        cfs.Add(new Cashflow(paymentDates[i], cfMultiplier * notionals[i] * accrualFractions[i] * rates[i], ccy));
                    }
                    else
                    {
                        cfs.Add(new Cashflow(paymentDates[i], -cfMultiplier * notionals[i] * (1 - recoveryRate), ccy));
                        break;
                    }
                }
            }            
            return cfs;
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            // Going to trust that this will not be called for an irrelevant index.
            return new List<Date> { paymentDates.Last() }; 
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable>() { defaultTime, defaultRecovery };
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
                throw new ArgumentException("Unknown index: " + index.ToString());
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
                if (paymentDates[i] > valueDate && (defaultTimeValue==null || paymentDates[i]<defaultTimeValue)) dates.Add(paymentDates[i]);
            }
            if (defaultTimeValue!=null && defaultTimeValue <= paymentDates.Last())
                dates.Add(defaultTimeValue);
            return dates;
        }
    }
}
