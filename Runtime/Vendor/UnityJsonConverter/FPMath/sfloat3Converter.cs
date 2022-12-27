#if FIXED_POINT_MATH

using ME.ECS.Mathematics;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity float3 type <see cref="float3"/>.
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class sfloat3Converter : PartialConverter<float3>
    {
        protected override void ReadValue(ref float3 value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.x):
                    value.x = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
                    break;
                case nameof(value.y):
                    value.y = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
                    break;
                case nameof(value.z):
                    value.z = sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, float3 value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.x));
            writer.WriteValue(value.x.RawValue);
            writer.WritePropertyName(nameof(value.y));
            writer.WriteValue(value.y.RawValue);
            writer.WritePropertyName(nameof(value.z));
            writer.WriteValue(value.z.RawValue);
        }
    }
}

#endif