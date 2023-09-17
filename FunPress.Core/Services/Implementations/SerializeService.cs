using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace FunPress.Core.Services.Implementations
{
    internal class SerializeService : ISerializeService
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings 
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            Converters = new List<JsonConverter> 
            {
                new StringEnumConverter()
            },
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string SerializeObject(object value)
        {
            return value == null ? null : JsonConvert.SerializeObject(value, _jsonSerializerSettings);
        }

        public T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, _jsonSerializerSettings);
        }
    }
}
