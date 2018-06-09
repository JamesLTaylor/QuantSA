namespace QuantSAInstaller
{
    internal enum Bitness
    {
        Bitness32,
        Bitness64
    }

    internal class BitInstance
    {
        public BitInstance(string data, Bitness bitness)
        {
            Data = data;
            Bitness = Bitness;
        }

        public string Data { get; }
        public Bitness Bitness { get; }
    }
}