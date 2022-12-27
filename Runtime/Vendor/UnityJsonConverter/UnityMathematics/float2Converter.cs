using Unity.Mathematics;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity float3 type <see cref="float3"/>.
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class float2Convertercs : PartialConverter<float2>
    {
        protected override void ReadValue(ref float2 value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.x):
                    value.x = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
                case nameof(value.y):
                    value.y = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, float2 value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.x));
            writer.WriteValue(value.x);
            writer.WritePropertyName(nameof(value.y));
            writer.WriteValue(value.y);
        }
    }
}
