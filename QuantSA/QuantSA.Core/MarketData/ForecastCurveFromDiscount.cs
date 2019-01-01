using QuantSA.Shared.Dates;
using QuantSA.Shared.MarketData;
using QuantSA.Shared.MarketObservables;

namespace QuantSA.Core.MarketData
{
    /// <summary>
    /// Wraps a discount curve as a forward rate forecasting curve.
    /// </summary>
    public class ForecastCurveFromDiscount : IFloatingRateSource
    {
        private readonly IDiscountingSource _discountCurve;
        private readonly IFloatingRateSource _fixingCurve;
        private readonly FloatRateIndex _index;
        private readonly string _name;


        /// <summary>
        /// Will use the discount factors to obtain the forward rates after the curve's anchor date and the fixing curve before
        /// that date.
        /// </summary>
        /// <param name="discountCurve"></param>
        /// <param name="index"></param>
        /// <param name="fixingCurve"></param>
        public ForecastCurveFromDiscount(IDiscountingSource discountCurve, FloatRateIndex index,
            IFloatingRateSource fixingCurve)
        {
            _discountCurve = discountCurve;
            _index = index;
            _fixingCurve = fixingCurve;
            _name = new FloatingRateSourceDescription(index).Name;
        }

        public FloatRateIndex GetFloatingIndex()
        {
            return _index;
        }

        /// <summary>
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public double GetForwardRate(Date date)
        {
            //TODO: Index should store the business day and daycount conventions of the index.            
            if (date > _discountCurve.GetAnchorDate())
            {
                var df1 = _discountCurve.GetDF(date);
                var laterDate = date.AddTenor(_index.Tenor);
                var df2 = _discountCurve.GetDF(laterDate);
                var dt = (laterDate - date) / 365.0;
                var fwdRate = (df1 / df2 - 1) / dt;
                return fwdRate;
            }

            return _fixingCurve.GetForwardRate(date);
        }

        public Date GetAnchorDate()
        {
            return _discountCurve.GetAnchorDate();
        }

        public string GetName()
        {
            return _name;
        }

        public bool CanBeA<T>(MarketDataDescription<T> marketDataDescription, IMarketDataContainer marketDataContainer)
            where T : class, IMarketDataSource
        {
            return marketDataDescription.Name == _name;
        }

        public T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource
        {
            return marketDataDescription.Name == _name ? this as T : null;
        }

        public bool TryCalibrate(Date calibrationDate, IMarketDataContainer marketDataContainer)
        {
            return true;
        }
    }
}