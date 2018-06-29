namespace QuantSA.Shared.Serialization
{
    /// <summary>
    /// An object of which there should only ever be one version.  These objects
    /// </summary>
    public interface ISerializableViaName
    {
        /// <summary>
        /// 
        /// </summary>
        string LookupName { get; }
    }
}