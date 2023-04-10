#if FIXED_POINT_MATH

using System;
using Saro.FPMath;

namespace Newtonsoft.Json.UnityConverters
{
    [UnityEngine.Scripting.Preserve]
    public class sfloatConverter : AutoJsonConverter<sfloat>
    {
        public override sfloat ReadJson(JsonReader reader, Type objectType, sfloat existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var rawValue = (uint)(long)reader.Value;
            existingValue = sfloat.FromRaw(rawValue);

            if (reader.TokenType == JsonToken.Comment)
                reader.Read();  // read comment

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, sfloat value, JsonSerializer serializer)
        {
            writer.WriteValue((long)value.rawValue);

#if ENABLE_NEWTONSOFT_JSON_COMMENT
            writer.WriteComment("(fp)" + value.ToString());
#endif
        }
    }
}

#endif