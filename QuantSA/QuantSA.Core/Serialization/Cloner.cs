using System.Collections.Generic;
using Newtonsoft.Json;

namespace QuantSA.Core.Serialization
{
    public class Cloner
    {
        public static object Clone(object input)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>
                {
                    new NameSerializer(),
                    new DateConverter()
                },
                ContractResolver = new AllFieldsContractResolver(),
                TypeNameHandling = TypeNameHandling.Auto,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };
            var json = JsonConvert.SerializeObject(input, settings);
            var obj = JsonConvert.DeserializeObject(json, input.GetType(), settings);
            return obj;
        }
    }
}