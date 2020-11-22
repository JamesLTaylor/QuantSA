using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using QuantSA.Core.Primitives;
using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;

namespace QuantSA.Core.Products.Credit
{
    /// <summary>
    /// A par type credit default swap whose cashflows depend explicitly on the default events.  
    /// Protection always applies from the value date: there is not concept in the class of a forward starting 
    /// CDS.
    /// </summary>
    /// <seealso cref="Product" />
    public class CDS : Product
    {
        private readonly double[] _accrualFractions;
        private readonly Currency _ccy;

        /// <summary>
        /// 1 when we have sold protection.  -1 when we have bought protection.
        /// </summary>
        private readonly double _cfMultiplier;

        // Market observables
        private readonly DefaultRecovery _defaultRecovery;
        private readonly DefaultTime _defaultTime;

        private readonly double[] _notionals;

        // Contract definition
        private readonly Date[] _paymentDates;
        private readonly double[] _rates;
        
        // Simulation values        
        [JsonIgnore] private Date _valueDate;
        [JsonIgnore] private Date _defaultTimeValue;
        [JsonIgnore] private double _recoveryRate;

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
            _defaultRecovery = new DefaultRecovery(refEntity);
            _defaultTime = new DefaultTime(refEntity);
            _ccy = ccy;
            _paymentDates = paymentDates;
            _notionals = notionals;
            _rates = rates;
            _accrualFractions = accrualFractions;
            _cfMultiplier = boughtProtection ? -1.0 : 1.0;
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
            for (var i = 0; i < _paymentDates.Length; i++)
                if (_paymentDates[i] > _valueDate)
                {
                    if (_paymentDates[i] < _defaultTimeValue)
                    {
                        cfs.Add(new Cashflow(_paymentDates[i],
                            _cfMultiplier * _notionals[i] * _accrualFractions[i] * _rates[i], _ccy));
                    }
                    else
                    {
                        cfs.Add(new Cashflow(_paymentDates[i], -_cfMultiplier * _notionals[i] * (1 - _recoveryRate), _ccy));
                        break;
                    }
                }

            return cfs;
        }

        public override List<Date> GetRequiredIndexDates(MarketObservable index)
        {
            // Going to trust that this will not be called for an irrelevant index.
            return new List<Date> {_paymentDates.Last()};
        }

        public override List<MarketObservable> GetRequiredIndices()
        {
            return new List<MarketObservable> {_defaultTime, _defaultRecovery};
        }

        public override void Reset()
        {
            _defaultTimeValue = null;
            _recoveryRate = double.NaN;
        }

        public override void SetIndexValues(MarketObservable index, double[] indexValues)
        {
            if (index == _defaultTime)
                _defaultTimeValue = new Date(indexValues[0]);
            else if (index == _defaultRecovery)
                _recoveryRate = indexValues[0];
            else
                throw new ArgumentException("Unknown index: " + index);
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
                if (_paymentDates[i] > _valueDate && (_defaultTimeValue == null || _paymentDates[i] < _defaultTimeValue))
                    dates.Add(_paymentDates[i]);
            if (_defaultTimeValue != null && _defaultTimeValue <= _paymentDates.Last())
                dates.Add(_defaultTimeValue);
            return dates;
        }
    }
}