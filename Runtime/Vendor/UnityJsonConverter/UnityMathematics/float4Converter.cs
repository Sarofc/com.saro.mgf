using Unity.Mathematics;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity float3 type <see cref="float4"/>.
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class float4Converter : PartialConverter<float4>
    {
        protected override void ReadValue(ref float4 value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.x):
                    value.x = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
                case nameof(value.y):
                    value.y = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
                case nameof(value.z):
                    value.z = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
                case nameof(value.w):
                    value.w = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, float4 value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.x));
            writer.WriteValue(value.x);
            writer.WritePropertyName(nameof(value.y));
            writer.WriteValue(value.y);
            writer.WritePropertyName(nameof(value.z));
            writer.WriteValue(value.z);
            writer.WritePropertyName(nameof(value.w));
            writer.WriteValue(value.w);
        }
    }
}
