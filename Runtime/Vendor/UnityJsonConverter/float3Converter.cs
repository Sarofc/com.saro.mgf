using Unity.Mathematics;
using UnityEngine;

namespace Newtonsoft.Json.UnityConverters
{
    // TODO test
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity float3 type <see cref="float3"/>.
    /// </summary>
    public class float3Converter : PartialConverter<float3>
    {
        protected override void ReadValue(ref float3 value, string name, JsonReader reader, JsonSerializer serializer)
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
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, float3 value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.x));
            writer.WriteValue(value.x);
            writer.WritePropertyName(nameof(value.y));
            writer.WriteValue(value.y);
            writer.WritePropertyName(nameof(value.z));
            writer.WriteValue(value.z);
        }
    }
}
