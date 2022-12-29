#if FIXED_POINT_MATH

using System;
using System.Diagnostics.CodeAnalysis;
using ME.ECS.Mathematics;

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

            //switch (name)
            //{
            //    case nameof(value.value.x):
            //        value.value.x = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
            //        break;
            //    case nameof(value.value.y):
            //        value.value.y = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
            //        break;
            //    case nameof(value.value.z):
            //        value.value.z = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
            //        break;
            //    case nameof(value.value.w):
            //        value.value.w = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
            //        break;
            //}
        }

        protected override void WriteJsonProperties(JsonWriter writer, quaternion value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.value.x));
            writer.WriteValue(value.value.x.RawValue);
            writer.WritePropertyName(nameof(value.value.y));
            writer.WriteValue(value.value.y.RawValue);
            writer.WritePropertyName(nameof(value.value.z));
            writer.WriteValue(value.value.z.RawValue);
            writer.WritePropertyName(nameof(value.value.w));
            writer.WriteValue(value.value.w.RawValue);

#if ENABLE_JSON_COMMENT
            writer.WriteComment("(fp)" + value.ToString());
#endif
        }
    }
}

#endif