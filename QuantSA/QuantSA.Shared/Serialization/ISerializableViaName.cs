namespace QuantSA.Shared.Serialization
{
    /// <summary>
    /// An object of which there should only ever be one version.  These objects get serialized via their name.
    /// Do not implement this interface rather extend <see cref="SerializableViaName"/> which has the equality
    /// properly implemented via the name.
    /// </summary>
    public interface ISerializableViaName
    {
        /// <summary>
        /// Get the name with which the object should be stored and serialized.
        /// </summary>
        string GetName();
    }
}