#if FIXED_POINT_MATH

using System;
using System.Diagnostics.CodeAnalysis;

namespace Newtonsoft.Json.UnityConverters
{
    [UnityEngine.Scripting.Preserve]
    public class sfloatConvertercs : AutoJsonConverter<sfloat>
    {
        public override sfloat ReadJson(JsonReader reader, Type objectType, sfloat existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            //if (existingValue is sfloat sfloat)
            //    return sfloat;
            //Log.ERROR($"Value 1: {reader.Value.ToString()} {reader.ValueType}");
            var rawValue = (uint)(long)reader.Value;

            if (reader.TokenType == JsonToken.Comment)
                reader.Read();  // read comment

            //Log.ERROR($"Value 2: {reader.Value.ToString()} {reader.ValueType}");
            return sfloat.FromRaw(rawValue);
        }

        public override void WriteJson(JsonWriter writer, sfloat value, JsonSerializer serializer)
        {
            writer.WriteValue(value.RawValue);

#if ENABLE_JSON_COMMENT
            writer.WriteComment("(fp)" + value.ToString());
#endif
        }

        //protected override void ReadValue(ref sfloat value, string name, JsonReader reader, JsonSerializer serializer)
        //{
        //    value = sfloat.FromRaw((uint?)reader.ReadAsInt32() ?? 0u);
        //}

        //protected override void WriteJsonProperties(JsonWriter writer, sfloat value, JsonSerializer serializer)
        //{
        //    //writer.WritePropertyName("raw");
        //    writer.WriteValue(value.RawValue); // TODO exception 是不是要写入propertyName？Var系列也有类似问题好像。
        //    //Newtonsoft.Json.Converters.StringEnumConverter // 参考这个
        //}
    }
}

#endif