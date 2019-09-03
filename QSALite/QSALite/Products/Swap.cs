using System.Collections.Generic;
using QSALite.Dates;
using QSALite.MarketObservables;

namespace QSALite.Products
{
    public class Swaplet
    {
        public readonly double AccrualFraction;
        public readonly Date PayDate;
        public readonly Date ResetDate;

        public Swaplet(Date resetDate, Date payDate, double accrualFraction)
        {
            ResetDate = resetDate;
            PayDate = payDate;
            AccrualFraction = accrualFraction;
        }
    }

    public class Swap : IProduct
    {
        private readonly double _fixedRate;
        private readonly FloatRateIndex _floatRateIndex;
        private readonly double _floatSpread;

        /// <summary>
        /// Positive means pay fixed.
        /// </summary>
        private readonly double _notional;

        private readonly Swaplet[] _swapLets;
        private readonly Date _valueDate;

        public Swap(Date valueDate, double notional, double fixedRate, double floatSpread,
            FloatRateIndex floatRateIndex, Swaplet[] swapLets)
        {
            _valueDate = valueDate;
            _notional = notional;
            _fixedRate = fixedRate;
            _floatSpread = floatSpread;
            _floatRateIndex = floatRateIndex;
            _swapLets = swapLets;
        }

        public List<Cashflow> GetCashflows(IMarketObservableProvider marketObservableProvider)
        {
            var cfs = new List<Cashflow>();
            foreach (var swaplet in _swapLets)
            {
                if (swaplet.PayDate.OnOrBefore(_valueDate)) continue;
                cfs.Add(new Cashflow(swaplet.PayDate, _notional * swaplet.AccrualFraction * _fixedRate,
                    _floatRateIndex.Currency));
                var floatRate = marketObservableProvider.GetValue(_floatRateIndex, swaplet.ResetDate);
                cfs.Add(new Cashflow(swaplet.PayDate, -_notional * swaplet.AccrualFraction * floatRate,
                    _floatRateIndex.Currency));
            }

            return cfs;
        }

        public IProduct CopyWithNewValueDate(Date newValueDate)
        {
            return new Swap(newValueDate, _notional, _fixedRate, _floatSpread, _floatRateIndex, _swapLets);
        }
    }
}