#if FIXED_POINT_MATH

using System;
using System.Diagnostics.CodeAnalysis;
using Saro.FPMath;

namespace Newtonsoft.Json.UnityConverters
{
    [UnityEngine.Scripting.Preserve]
    public class sfloat3Converter : AutoPartialConverter<float3>
    {
        [return: MaybeNull]
        public override object ReadJson(JsonReader reader, Type objectType, [AllowNull] object existingValue, JsonSerializer serializer)
        {
            var obj = base.ReadJson(reader, objectType, existingValue, serializer);

            if (reader.TokenType == JsonToken.Comment)
                reader.Read(); // comment;

            return obj;
        }

        protected override void ReadValue(ref float3 value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.x):
                    reader.Read();
                    value.x = sfloat.FromRaw((uint)(long)reader.Value);
                    break;
                case nameof(value.y):
                    reader.Read();
                    value.y = sfloat.FromRaw((uint)(long)reader.Value);
                    break;
                case nameof(value.z):
                    reader.Read();
                    value.z = sfloat.FromRaw((uint)(long)reader.Value);
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, float3 value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.x));
            writer.WriteValue((long)value.x.rawValue);
            writer.WritePropertyName(nameof(value.y));
            writer.WriteValue((long)value.y.rawValue);
            writer.WritePropertyName(nameof(value.z));
            writer.WriteValue((long)value.z.rawValue);

#if ENABLE_NEWTONSOFT_JSON_COMMENT
            writer.WriteComment("(fp)" + value.ToString());
#endif
        }
    }
}

#endif