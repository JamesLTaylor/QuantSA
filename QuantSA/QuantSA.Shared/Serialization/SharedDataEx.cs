using System;
using QuantSA.Shared.Primitives;

namespace QuantSA.Shared.Serialization
{
    public static class SharedDataEx
    {
        public static T Get<T>(this ISharedData sharedData, string name) where T : ISerializableViaName
        {
            if (!sharedData.TryGet(typeof(T), name, out var obj))
                throw new ArgumentException($"There is no shared instance of {typeof(T).Name} with name {name}");
            return (T) obj;
        }

        public static Currency GetCurrency(this ISharedData sharedData, string name)
        {
            return Get<Currency>(sharedData, name);
        }

        public static bool TryGet<T>(this ISharedData sharedData, string name, out T obj) where T : ISerializableViaName
        {
            obj = default(T);
            if (!sharedData.TryGet(typeof(T), name, out var serializableViaName))
                return false;
            obj = (T) serializableViaName;
            return true;
        }

        public static ISerializableViaName Get(this ISharedData sharedData, Type type, string name)
        {
            if (!sharedData.TryGet(type, name, out var serializableViaName))
                throw new ArgumentException($"There is no shared instance of {type.Name} with name {name}");
            return serializableViaName;
        }
    }
}