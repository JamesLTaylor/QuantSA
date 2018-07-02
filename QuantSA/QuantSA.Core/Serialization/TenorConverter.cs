using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantSA.Shared.Dates;

namespace QuantSA.Core.Serialization
{
    public class TenorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Tenor);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var tenorStr = JToken.Load(reader).ToString();
            return string.IsNullOrWhiteSpace(tenorStr) ? null : new Tenor(tenorStr);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) return;
            var tenor = value as Tenor;
            serializer.Serialize(writer, tenor.ToString());
        }
    }
}