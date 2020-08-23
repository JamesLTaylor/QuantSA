using System;
using System.Collections.Generic;
using QuantSA.Shared.Serialization;

namespace QuantSA.CoreExtensions.Data
{
    /// <summary>
    /// A storage for shared data definitions.
    /// </summary>
    public class SharedData : ISharedData
    {
        private readonly Dictionary<Type, Dictionary<string, ISerializableViaName>> _typeNameAndInstances =
            new Dictionary<Type, Dictionary<string, ISerializableViaName>>();

        public bool TryGet(Type type, string name, out ISerializableViaName serializableViaName)
        {
            serializableViaName = null;
            if (!TryGetValueForType(type, out var dictForType)) return false;
            return dictForType.TryGetValue(name, out serializableViaName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dictForType"></param>
        /// <returns></returns>
        private bool TryGetValueForType(Type type, out Dictionary<string, ISerializableViaName> dictForType)
        {
            foreach (var index in _typeNameAndInstances)
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
            if (!_typeNameAndInstances.TryGetValue(instance.GetType(), out var dictForType))
            {
                dictForType = new Dictionary<string, ISerializableViaName>();
                _typeNameAndInstances[instance.GetType()] = dictForType;
            }

            //if (dictForType.ContainsKey(instance.GetName()))
            //    throw new ArgumentException(
            //        $"Shared instance of {instance.GetType().Name} with name {instance.GetName()} has already been set.");
            dictForType[instance.GetName()] = instance;
        }
    }
}