using System;
using log4net;
using QuantSA.Shared.MarketObservables;
using QuantSA.Shared.Primitives;
using QuantSA.Shared.Serialization;

namespace QuantSA.Shared.State
{
    /// <summary>
    /// The global state of the QuantSA library. This should only be used for static (or nearly static) data and
    /// shared resources like loggers. It should not be used to pass data in or out of calculations.
    /// </summary>
    public static class QuantSAState
    {
        private static ILogFactory _logFactory;
        public static ISharedData SharedData { get; private set; }

        public static void SetSharedData(ISharedData sharedData)
        {
            SharedData = sharedData;
        }

        public static ILogFactory LogFactory => _logFactory ?? new EmptyLogFactory();


        public static void SetLogger(ILogFactory logger)
        {
            _logFactory = logger;
        }


        /// <summary>
        /// Load a <see cref="Currency"/> from the <see cref="SharedData"/> if it has been loaded.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Currency GetCurrency(string name)
        {
            return SharedData.Get<Currency>(name);
        }

        /// <summary>
        /// Load a <see cref="FloatRateIndex"/> from the <see cref="SharedData"/> if it has been loaded.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FloatRateIndex GetFloatRateIndex(string name)
        {
            return SharedData.Get<FloatRateIndex>(name);
        }

    }

public class EmptyLogFactory : ILogFactory
    {
        public ILog Get(Type type)
        {
            return LogManager.GetLogger(type);
        }
    }
}