using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantSA.Shared.Serialization;

namespace QuantSA.Core.Serialization
{
    /// <summary>
    /// Takes any object that implements <see cref="ISerializableViaName"/> and ensures that it is serialized
    /// only as its name.  Deserialization then occurs by looking for an instance of the required type with the
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
            var jsonObject = JObject.Load(reader);
            var properties = jsonObject.Properties().ToList();
            var name = (string) properties[0].Value;
            return QuantSAState.SharedData.Get(objectType, name);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var objWithName = value as ISerializableViaName;
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            serializer.Serialize(writer, objWithName.LookupName);
            writer.WriteEndObject();
        }
    }
}