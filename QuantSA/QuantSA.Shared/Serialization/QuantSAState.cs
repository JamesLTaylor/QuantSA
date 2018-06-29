namespace QuantSA.Shared.Serialization
{
    public static class QuantSAState
    {
        public static ISharedData SharedData { get; private set; }

        public static void SetSharedData(ISharedData sharedData)
        {
            SharedData = sharedData;
        }
    }
}