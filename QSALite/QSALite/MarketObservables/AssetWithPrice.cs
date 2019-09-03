namespace QSALite
{
    /// <summary>
    /// Something that has a price in units of <see cref="QuoteAsset" />. For example
    /// the value of a share ABC in ZAR will have <see cref="Asset" /> as a <see cref="Share" />
    /// with name "ABC" and <see cref="QuoteAsset" /> as a currency with name "ZAR"
    /// </summary>
    public class AssetWithPrice
    {
        public Asset Asset;
        public Asset QuoteAsset;
    }
}