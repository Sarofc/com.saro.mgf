#if FIXED_POINT_MATH

using System;
using Newtonsoft.Json.Linq;
using Saro;

namespace Newtonsoft.Json.UnityConverters
{
    [UnityEngine.Scripting.Preserve]
    public class sfloatConvertercs : PartialConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(sfloat) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (existingValue is sfloat sfloat)
                return sfloat;

            return sfloat.FromRaw((uint?)reader.ReadAsDecimal() ?? 0u);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is sfloat _sfloat)
            {
                writer.WriteValue(_sfloat.RawValue);
                writer.WriteComment("(fp)" + _sfloat.ToString());
            }
            else
            {
                writer.WriteNull();
            }
        }

        //protected override void ReadValue(ref sfloat value, string name, JsonReader reader, JsonSerializer serializer)
        //{
        //    value = sfloat.FromRaw((uint?)reader.ReadAsInt32() ?? 0u);
        //}

        //protected override void WriteJsonProperties(JsonWriter writer, sfloat value, JsonSerializer serializer)
        //{
        //    //writer.WritePropertyName("raw");
        //    writer.WriteValue(value.RawValue); // TODO exception 是不是要写入propertyName？Var系列也有类似问题好像。
        //    //Newtonsoft.Json.Converters.StringEnumConverter // 参考这个
        //}
    }
}

#endif