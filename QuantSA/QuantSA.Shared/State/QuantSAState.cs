using log4net;
using QuantSA.Shared.Serialization;

namespace QuantSA.Shared.State
{
    public static class QuantSAState
    {
        public static ISharedData SharedData { get; private set; }

        public static void SetSharedData(ISharedData sharedData)
        {
            SharedData = sharedData;
        }

        public static ILogFactory LogFactory { get; private set; }

        public static void SetLogger(ILogFactory logger)
        {
            LogFactory = logger;
        }
    }
}