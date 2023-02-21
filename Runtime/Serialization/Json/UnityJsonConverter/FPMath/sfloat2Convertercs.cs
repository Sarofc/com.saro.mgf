#if FIXED_POINT_MATH

using System;
using System.Diagnostics.CodeAnalysis;
using Saro.FPMath;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity float3 type <see cref="float3"/>.
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class sfloat2Convertercs : AutoPartialConverter<float2>
    {
        public override object ReadJson(JsonReader reader, Type objectType, [AllowNull] object existingValue, JsonSerializer serializer)
        {
            var obj = base.ReadJson(reader, objectType, existingValue, serializer);

            if (reader.TokenType == JsonToken.Comment)
                reader.Read(); // comment;

            return obj;
        }

        protected override void ReadValue(ref float2 value, string name, JsonReader reader, JsonSerializer serializer)
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
            }

            //switch (name)
            //{
            //    case nameof(value.x):
            //        value.x = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
            //        break;
            //    case nameof(value.y):
            //        value.y = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
            //        break;
            //}
        }

        protected override void WriteJsonProperties(JsonWriter writer, float2 value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.x));
            writer.WriteValue(value.x.rawValue);
            writer.WritePropertyName(nameof(value.y));
            writer.WriteValue(value.y.rawValue);

#if ENABLE_JSON_COMMENT
            writer.WriteComment("(fp)" + value.ToString());
#endif
        }
    }
}

#endif