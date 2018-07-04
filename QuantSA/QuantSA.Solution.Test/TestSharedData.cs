using System;
using System.Collections.Generic;
using QuantSA.Shared.Serialization;

namespace QuantSA.Solution.Test
{
    public class TestSharedData : ISharedData
    {
        private readonly Dictionary<Type, Dictionary<string, ISerializableViaName>> TypeNameAndInstances =
            new Dictionary<Type, Dictionary<string, ISerializableViaName>>();

        public bool TryGet(Type type, string name, out ISerializableViaName serializableViaName)
        {
            serializableViaName = null;
            if (!TryGetValueForType(type, out var dictForType)) return false;
            return dictForType.TryGetValue(name, out serializableViaName);
        }

        public void TempAdd(ISerializableViaName serializableViaName)
        {
            Set(serializableViaName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dictForType"></param>
        /// <returns></returns>
        private bool TryGetValueForType(Type type, out Dictionary<string, ISerializableViaName> dictForType)
        {
            foreach (var index in TypeNameAndInstances)
            {
                if (!type.IsAssignableFrom(index.Key)) continue;
                dictForType = index.Value;
                return true;
            }

            dictForType = null;
            return false;
        }

        public void Set(params ISerializableViaName[] instances)
        {
            foreach (var instance in instances) Set(instance);
        }

        public void Set(ISerializableViaName instance)
        {
            if (!TypeNameAndInstances.TryGetValue(instance.GetType(), out var dictForType))
            {
                dictForType = new Dictionary<string, ISerializableViaName>();
                TypeNameAndInstances[instance.GetType()] = dictForType;
            }

            if (dictForType.ContainsKey(instance.GetName()))
                throw new ArgumentException(
                    $"Shared instance of {instance.GetType().Name} with name {instance.GetName()} has already been set.");
            dictForType[instance.GetName()] = instance;
        }
    }
}