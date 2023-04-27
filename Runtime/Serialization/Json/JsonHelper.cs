#if PACKAGE_NEWTONSOFT_BSON
using Newtonsoft.Json.Bson;
#endif

using System;
using Newtonsoft.Json.UnityConverters;
using Newtonsoft.Json;
using System.IO;

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
            MissingMemberHandling = MissingMemberHandling.Ignore,
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

#if PACKAGE_NEWTONSOFT_BSON
        public static void ToBson(Stream stream, object obj, DateTimeKind dateTimeKindHandling = DateTimeKind.Local, JsonSerializerSettings settings = null)
        {
            using var reader = new BsonDataWriter(stream);
            reader.DateTimeKindHandling = dateTimeKindHandling;
            var serializer = JsonSerializer.CreateDefault(settings ?? s_DefaultSettings);
            serializer.Serialize(reader, obj);
        }

        public static T FromBson<T>(Stream stream, bool readRootValueAsArray = false, DateTimeKind dateTimeKindHandling = DateTimeKind.Local, JsonSerializerSettings settings = null)
        {
            using var reader = new BsonDataReader(stream, readRootValueAsArray, dateTimeKindHandling);
            var serializer = JsonSerializer.CreateDefault(settings ?? s_DefaultSettings);
            return serializer.Deserialize<T>(reader);
        }
#endif
    }
}
