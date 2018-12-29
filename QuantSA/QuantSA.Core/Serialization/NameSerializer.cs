using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantSA.Shared.Serialization;
using QuantSA.Shared.State;

namespace QuantSA.Core.Serialization
{
    /// <summary>
    /// Takes any object that implements <see cref="ISerializableViaName"/> and ensures that it is serialized
    /// only as its name.  De-serialization then occurs by looking for an instance of the required type with the
    /// same name in <see cref="QuantSAState"/>.
    /// </summary>
    public class NameSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ISerializableViaName).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var name = JToken.Load(reader).ToString();
            return string.IsNullOrWhiteSpace(name) ? null : QuantSAState.SharedData.Get(objectType, name);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) return;
            var objWithName = value as ISerializableViaName;
            serializer.Serialize(writer, objWithName.GetName());
            if (!QuantSAState.SharedData.TryGet(value.GetType(), objWithName.GetName(), out _))
                QuantSAState.SharedData.TempAdd(objWithName);
        }
    }
}