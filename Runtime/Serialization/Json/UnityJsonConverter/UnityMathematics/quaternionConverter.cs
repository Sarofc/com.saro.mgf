using Unity.Mathematics;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// Custom Newtonsoft.Json converter <see cref="JsonConverter"/> for the Unity float3 type <see cref="quaternion"/>.
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class quaternionConverter : AutoPartialConverter<quaternion>
    {
        protected override void ReadValue(ref quaternion value, string name, JsonReader reader, JsonSerializer serializer)
        {
            switch (name)
            {
                case nameof(value.value.x):
                    value.value.x = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
                case nameof(value.value.y):
                    value.value.y = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
                case nameof(value.value.z):
                    value.value.z = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
                case nameof(value.value.w):
                    value.value.w = (float?)reader.ReadAsDouble() ?? 0f;
                    break;
            }
        }

        protected override void WriteJsonProperties(JsonWriter writer, quaternion value, JsonSerializer serializer)
        {
            writer.WritePropertyName(nameof(value.value.x));
            writer.WriteValue(value.value.x);
            writer.WritePropertyName(nameof(value.value.y));
            writer.WriteValue(value.value.y);
            writer.WritePropertyName(nameof(value.value.z));
            writer.WriteValue(value.value.z);
            writer.WritePropertyName(nameof(value.value.w));
            writer.WriteValue(value.value.w);
        }
    }
}
