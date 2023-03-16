#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Saro;
using UnityEditor;
using UObject = UnityEngine.Object;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// 将 <seealso cref="UObject"/> 类型的资源 序列化为 guid string，仅限编辑器使用
    /// <code>[JsonConverter(typeof(UObjectConverter))]</code>
    /// <code>List`UObject`or UObject[]  or UObject</code>
    /// </summary>
    public class UObjectToGUIDConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(UObject).IsAssignableFrom(objectType)
                || typeof(IEnumerable<UObject>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // JValue/JObject/JArray 默认忽略 comment，所以不需要宏

            if (typeof(UObject).IsAssignableFrom(objectType))
            {
                var jToken = JToken.ReadFrom(reader);
                return ReadUObject(jToken);
            }
            else if (typeof(IEnumerable<UObject>).IsAssignableFrom(objectType))
            {
                var jArray = JArray.Load(reader);

                if (jArray.Count > 0)
                {
                    IList list = null;
                    if (!objectType.IsArray)
                        list = (IList)Activator.CreateInstance(objectType);
                    else
                        list = new ArrayList();

                    foreach (var jToken in jArray)
                    {
                        //Log.ERROR($"jToken: {jToken} {jToken.GetType()}");

                        var uobj = ReadUObject(jToken);
                        list.Add(uobj);
                    }

                    if (!objectType.IsArray)
                    {
                        return list;
                    }
                    else
                    {
                        var type = objectType.GetElementType();
                        var instance = Array.CreateInstance(type, list.Count);
                        list.CopyTo(instance, 0);
                        return instance;
                    }
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is UObject uobj)
            {
                WriteUObject(writer, uobj);
            }
            else if (value is IEnumerable<UObject> uobjs)
            {
                writer.WriteStartArray();

                foreach (var item in uobjs)
                {
                    WriteUObject(writer, item);
                }

                writer.WriteEndArray();
            }
        }

        private UObject ReadUObject(JToken jToken)
        {
            try
            {
                // 不要type了，AssetDatabase.LoadAssetAtPath 能够知道 加载的类型
                var guid = jToken["$guid"].Value<string>();
                var path = AssetDatabase.GUIDToAssetPath(guid);
                return AssetDatabase.LoadAssetAtPath(path, typeof(UObject));
            }
            catch (Exception e)
            {
                Log.ERROR(jToken.ToString() + "\n" + e);
            }
            return null;
        }

        private void WriteUObject(JsonWriter writer, UObject uobj)
        {
            writer.WriteStartObject();
            {
                var path = AssetDatabase.GetAssetPath(uobj);
                var guid = AssetDatabase.AssetPathToGUID(path);
                writer.WritePropertyName("$guid");
                writer.WriteValue(guid);

#if ENABLE_NEWTONSOFT_JSON_COMMENT && false // ctrl + k serarh guid
                writer.WriteComment(path);
#endif
            }
            writer.WriteEndObject();
        }
    }
}

#endif