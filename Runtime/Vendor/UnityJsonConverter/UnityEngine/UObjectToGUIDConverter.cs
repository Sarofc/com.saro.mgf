#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Saro;
using Saro.Utility;
using UnityEditor;
using UObject = UnityEngine.Object;

namespace Newtonsoft.Json.UnityConverters
{
    /// <summary>
    /// <seealso cref="UObject"/> 序列化为 guid string，仅限编辑器使用
    /// <code>usage [JsonConverter(typeof(UObjectConverter))]</code>
    /// </summary>
    public class UObjectToGUIDConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(UObject).IsAssignableFrom(objectType)
                || typeof(IEnumerable<UObject>).IsAssignableFrom(objectType);
            //return s_UObjectTypes.Contains(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (typeof(UObject).IsAssignableFrom(objectType))
            {
                var jObject = JObject.Load(reader);
                //if (jObject == null)
                //{
                //    Log.ERROR($"objectType: {objectType} reader.Path: {reader.Path} reader.Value: {reader.Value}");
                //    return existingValue;
                //}
                return ReadUObject(jObject);
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

                    foreach (var jObject in jArray)
                    {
                        var uobj = ReadUObject(jObject);
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
                        //for (int i = 0; i < list.Count; i++)
                        //{
                        //    instance.SetValue(list[i], i);
                        //}
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
            else
            {
                writer.WriteNull();
            }
        }

        private UObject ReadUObject(JToken jObject)
        {
            try
            {
                var typeInfo = jObject["$_type"].ToString();
                var guid = jObject["guid"].ToString();

                var type = TypeUtility.GetTypeByTypeInfo(typeInfo);
                var path = AssetDatabase.GUIDToAssetPath(guid);

                //Log.ERROR($"[Array] UObjectToGUIDConverter. objectType: {objectType} existingValue: {existingValue} typeName: {typeInfo} guid: {guid} type: {type}");

                return AssetDatabase.LoadAssetAtPath(path, type);
            }
            catch (Exception e)
            {
                Log.ERROR(jObject.ToString() + "\n" + e);
            }
            return null;
        }

        private void WriteUObject(JsonWriter writer, UObject uobj)
        {
            writer.WriteStartObject();
            {
                //serializer.TypeNameHandling

                writer.WritePropertyName("$_type");
                writer.WriteValue(TypeUtility.GetTypeInfo(uobj.GetType()));

                var path = AssetDatabase.GetAssetPath(uobj);
                var guid = AssetDatabase.AssetPathToGUID(path);
                writer.WritePropertyName("guid");
                writer.WriteValue(guid);

                writer.WriteComment(path);
            }
            writer.WriteEndObject();
        }
    }
}

#endif