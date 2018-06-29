using System;
using System.Runtime.Remoting.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantSA.Shared.Dates;

namespace QuantSA.Core.Serialization
{
    public class DateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Date);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var dateStr = JToken.Load(reader).ToString();
            return string.IsNullOrWhiteSpace(dateStr) ? null : new Date(dateStr);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) return;
            var date = value as Date;
            serializer.Serialize(writer, date.ToString());
        }
    }
}