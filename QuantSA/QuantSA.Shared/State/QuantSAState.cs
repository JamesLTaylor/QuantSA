using System;
using log4net;
using QuantSA.Shared.Serialization;

namespace QuantSA.Shared.State
{
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
    }

    public class EmptyLogFactory : ILogFactory
    {
        public ILog Get(Type type)
        {
            return LogManager.GetLogger(type);
        }
    }
}