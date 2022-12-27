#if FIXED_POINT_MATH

using ME.ECS.Mathematics;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity float3 type <see cref="quaternion"/>.
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class squaternionConverter : PartialConverter<quaternion>
    {
        protected override void ReadValue(ref quaternion value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.value.x):
                    value.value.x = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
                    break;
                case nameof(value.value.y):
                    value.value.y = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
                    break;
                case nameof(value.value.z):
                    value.value.z = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
                    break;
                case nameof(value.value.w):
                    value.value.w = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
                    break;
            }
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
        }
    }
}

#endif