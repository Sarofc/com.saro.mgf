﻿#if FIXED_POINT_MATH

using System;
using System.Diagnostics.CodeAnalysis;
using Saro;
using Saro.FPMath;
using UnityEngine.Rendering.Universal;

namespace Newtonsoft.Json.UnityConverters
{
    [UnityEngine.Scripting.Preserve]
    public class sfloatConvertercs : AutoJsonConverter<sfloat>
    {
        public override sfloat ReadJson(JsonReader reader, Type objectType, sfloat existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            //Log.ERROR($"Value 1: {reader.Value.ToString()} {reader.ValueType}");
            var rawValue = (uint)(long)reader.Value;
            existingValue = sfloat.FromRaw(rawValue);

            if (reader.TokenType == JsonToken.Comment)
                reader.Read();  // read comment

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, sfloat value, JsonSerializer serializer)
        {
            writer.WriteValue(value.rawValue);

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