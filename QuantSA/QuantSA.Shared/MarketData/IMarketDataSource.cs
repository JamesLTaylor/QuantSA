using QuantSA.Shared.Dates;

namespace QuantSA.Shared.MarketData
{
    /// <summary>
    /// Common interface implemented by all market data sources.  These can be things such as expectations of
    /// tradable instruments like forward rates, specific model parameters or discount sources.
    /// </summary>
    /// <remarks>
    /// An implied FX cross forward curve would do nothing during calibration except store a reference to the
    /// <see cref="IMarketDataContainer" /> and would return true to <see cref="CanBeA{T}" /> and when queried for forward
    /// rates would collect the curves it needs.
    /// </remarks>
    public interface IMarketDataSource
    {
        Date GetAnchorDate();

        /// <summary>
        /// Only used to give sensible error messages.  When it makes sense consider getting the name from a
        /// <see cref="MarketDataDescription{T}.Name" /> to help users easily understand what type of curve this
        /// is.
        /// </summary>
        string GetName();

        /// <summary>
        /// Check if this instance of a <see cref="IMarketDataSource" /> can be used by someone
        /// requiring <paramref name="marketDataDescription" />
        /// </summary>
        /// <param name="marketDataDescription"></param>
        /// <param name="marketDataContainer"></param>
        /// <returns></returns>
        bool CanBeA<T>(MarketDataDescription<T> marketDataDescription, IMarketDataContainer marketDataContainer)
            where T : class, IMarketDataSource;

        /// <summary>
        /// Get a curve that can act uniquely as the requested type. When the <see cref="IMarketDataSource"/> is a
        /// set of curves then a single one is returned.  When the <see cref="IMarketDataSource"/> is a single
        /// curve returns itself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="marketDataDescription"></param>
        /// <returns></returns>
        T Get<T>(MarketDataDescription<T> marketDataDescription) where T : class, IMarketDataSource;

        /// <summary>
        /// Calibrate by mutating the current instance.
        /// </summary>
        /// <param name="calibrationDate"></param>
        /// <param name="marketDataContainer"></param>
        /// <returns></returns>
        bool TryCalibrate(Date calibrationDate, IMarketDataContainer marketDataContainer);
    }
}