using System;

namespace QuantSA.Shared.Serialization
{
    public interface ISharedData
    {
        bool TryGet(Type type, string name, out ISerializableViaName serializableViaName);

        /// <summary>
        /// Adds an <see cref="ISerializableViaName"/>.
        /// </summary>
        /// <param name="serializableViaName"></param>
        void Set(ISerializableViaName serializableViaName);
    }
}