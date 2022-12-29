using System;
using Newtonsoft.Json.UnityConverters;
using Newtonsoft.Json;

namespace Saro.Utility
{
    public class JsonHelper
    {
        static JsonSerializerSettings s_DefaultSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            //Formatting = Formatting.Indented,
            Converters = IAutoJsonConverter.GetJsonConverters(),
        };

        public static string ToJson(object obj, JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(obj, settings ?? s_DefaultSettings);
        }

        public static T FromJson<T>(string json, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<T>(json, settings ?? s_DefaultSettings);
        }

        public static object FromJson(string json, Type type, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject(json, type, settings ?? s_DefaultSettings);
        }
    }
}
