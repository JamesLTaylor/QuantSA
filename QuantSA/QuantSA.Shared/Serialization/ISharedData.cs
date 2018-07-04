using System;

namespace QuantSA.Shared.Serialization
{
    public interface ISharedData
    {
        bool TryGet(Type type, string name, out ISerializableViaName serializableViaName);
        void TempAdd(ISerializableViaName serializableViaName);
    }
}