#if FIXED_POINT_MATH

using System;
using System.Diagnostics.CodeAnalysis;
using Saro.FPMath;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity float3 type <see cref="quaternion"/>.
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class squaternionConverter : AutoPartialConverter<quaternion>
    {
        [return: MaybeNull]
        public override object ReadJson(JsonReader reader, Type objectType, [AllowNull] object existingValue, JsonSerializer serializer)
        {
            var obj = base.ReadJson(reader, objectType, existingValue, serializer);

            if (reader.TokenType == JsonToken.Comment)
                reader.Read(); // comment;

            return obj;
        }

        protected override void ReadValue(ref quaternion value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.value.x):
                    reader.Read();
                    value.value.x = sfloat.FromRaw((uint)(long)reader.Value);
                    break;
                case nameof(value.value.y):
                    reader.Read();
                    value.value.y = sfloat.FromRaw((uint)(long)reader.Value);
                    break;
                case nameof(value.value.z):
                    reader.Read();
                    value.value.z = sfloat.FromRaw((uint)(long)reader.Value);
                    break;
                case nameof(value.value.w):
                    reader.Read();
                    value.value.w = sfloat.FromRaw((uint)(long)reader.Value);
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, quaternion value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.value.x));
            writer.WriteValue((long)value.value.x.rawValue);
            writer.WritePropertyName(nameof(value.value.y));
            writer.WriteValue((long)value.value.y.rawValue);
            writer.WritePropertyName(nameof(value.value.z));
            writer.WriteValue((long)value.value.z.rawValue);
            writer.WritePropertyName(nameof(value.value.w));
            writer.WriteValue((long)value.value.w.rawValue);

#if ENABLE_NEWTONSOFT_JSON_COMMENT
            writer.WriteComment("(fp)" + value);
#endif
        }
    }
}

#endif