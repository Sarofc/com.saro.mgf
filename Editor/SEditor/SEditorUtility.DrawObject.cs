#if UNITY_EDITOR

#if FIXED_POINT_MATH
using sfloat = Saro.FPMath.sfloat;
using sfloat2 = Saro.FPMath.float2;
using sfloat3 = Saro.FPMath.float3;
using sfloat4 = Saro.FPMath.float4;
using squaternion = Saro.FPMath.quaternion;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Saro.Utility;
using Unity.Mathematics;

namespace Saro.SEditor
{
    /// <summary>
    /// Flavor GUI and AutomaticInspector function
    /// </summary>
    public static partial class SEditorUtility
    {
        private static GUIContent s_Temp = new();
        ///Custom Object and Attribute Drawers
        private static Dictionary<Type, ObjectDrawer> objectDrawers = new Dictionary<Type, ObjectDrawer>();
        static ObjectDrawer GetCustomDrawer(Type type)
        {
            if (objectDrawers.TryGetValue(type, out var result))
            {
                return result;
            }

            foreach (var drawerType in TypeUtility.GetSubClassTypesAllAssemblies(typeof(ObjectDrawer)))
            {
                var args = drawerType.BaseType.GetGenericArguments();
                if (args.Length == 1 && args[0].IsAssignableFrom(type))
                {
                    return objectDrawers[type] = Activator.CreateInstance(drawerType) as ObjectDrawer;
                }
            }

            return objectDrawers[type] = new NoDrawer();
        }

        // TODO stack overflow 检测
        //Show an automatic editor gui for arbitrary objects, taking into account custom attributes
        public static void ShowAutoEditorGUI(object o)
        {
            if (o == null) return;

            //Preliminary Hides
            if (typeof(Delegate).IsAssignableFrom(o.GetType())) return;

            foreach (var field in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var result = GenericField(field.Name, field.GetValue(o), field.FieldType, field, o);
                if (result.draw)
                    field.SetValue(o, result.obj);
            }

            foreach (var field in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (field.GetCustomAttribute<SerializeField>() != null || field.GetCustomAttribute<ShowInInspectorAttribute>() != null)
                {
                    var result = GenericField(field.Name, field.GetValue(o), field.FieldType, field, o);
                    if (result.draw)
                        field.SetValue(o, result.obj);
                }
            }

            GUI.enabled = Application.isPlaying;
            foreach (var prop in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    if (prop.DeclaringType.GetField("<" + prop.Name + ">k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance) != null)
                    {
                        GenericField(prop.Name, prop.GetValue(o, null), prop.PropertyType, prop, o);
                    }
                }
            }
            GUI.enabled = true;
        }

        //For generic automatic editors. Passing a MemberInfo will also check for attributes
        public static (bool draw, object obj) GenericField(string name, object value, Type t, MemberInfo member = null, object context = null)
        {
            name = ObjectNames.NicifyVariableName(name);

            if (t == null)
            {
                GUILayout.Label("NO TYPE PROVIDED!");
                return (false, value);
            }

            //Preliminary Hides
            if (typeof(Delegate).IsAssignableFrom(t))
            {
                return (false, value);
            }

            IEnumerable<Attribute> attributes = Array.Empty<Attribute>();
            if (member != null)
            {
                //Hide class?
                if (t.GetCustomAttributes(typeof(HideInInspector), true).FirstOrDefault() != null)
                {
                    return (false, value);
                }

                attributes = member.GetCustomAttributes<Attribute>(true);

                //Hide field?
                if (attributes.Any(a => a is HideInInspector))
                {
                    return (false, value);
                }

                // space
                {
                    var height = 0f;
                    foreach (var attribute in attributes)
                    {
                        if (attribute is SpaceAttribute _attr)
                        {
                            height += _attr.height;
                        }
                    }
                    if (height > 0f)
                        EditorGUILayout.Space(height);
                }

                if (attributes.Any(a => a is SeparatorAttribute))
                {
                    SEditorUtility.Separator();
                }

                //Is required?
                //if (attributes.Any(a => a is RequiredFieldAttribute))
                //{
                //    if ((value == null || value.Equals(null)) ||
                //        (t == typeof(string) && string.IsNullOrEmpty((string)value)) ||
                //        (typeof(BBParameter).IsAssignableFrom(t) && (value as BBParameter).isNull))
                //    {
                //        GUI.backgroundColor = lightRed;
                //    }
                //}
            }

            bool drawReference = false;
            if (member != null)
            {
                //var nameAtt = attributes.FirstOrDefault(a => a is NameAttribute) as NameAttribute;
                //if (nameAtt != null)
                //{
                //    name = nameAtt.name;
                //}

                if (context != null)
                {
                    if (attributes.Any(a => a is SerializeReference))
                    {
                        drawReference = true;
                    }

                    var showAtt = attributes.FirstOrDefault(a => a is ShowIfAttribute) as ShowIfAttribute;
                    if (showAtt != null)
                    {
                        var targetProperty = context.GetType().GetProperty(showAtt.requiredPropertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                        if (targetProperty == null/* || targetProperty.PropertyType != typeof(bool)*/)
                        {
                            GUILayout.Label(string.Format("[ShowIf] Error: \"{0}\" does not exist.", showAtt.requiredPropertyName));
                        }
                        else
                        {
                            var returnObject = targetProperty.GetValue(context);

                            if (returnObject == null)
                                return (false, value);

                            if (!returnObject.Equals(showAtt.checkObject))
                            {
                                return (false, value);
                            }
                        }
                    }
                }
            }

            //Before everything check BBParameter
            //if (typeof(BBParameter).IsAssignableFrom(t))
            //{
            //    return BBParameterField(name, (BBParameter)value, false, member, context);
            //}

            //Custom object drawers
            var objectDrawer = GetCustomDrawer(t);
            if (objectDrawer != null && !(objectDrawer is NoDrawer))
            {
                /*
                                var field = member as FieldInfo;
                                if (field != null && typeof(BBParameter).IsAssignableFrom(field.FieldType) ){
                                    var bbParam = field.GetValue(context);
                                    context = bbParam;
                                    member = bbParam.GetType().GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
                                }
                */
                var obj = objectDrawer.DrawGUI(name, value, member as FieldInfo, null, context);
                return (true, obj);
            }

            //Custom attribute drawers
            foreach (var att in attributes.OfType<CustomDrawerAttribute>())
            {
                var attributeDrawer = GetCustomDrawer(att.GetType());
                if (attributeDrawer != null && !(attributeDrawer is NoDrawer))
                {
                    var obj = attributeDrawer.DrawGUI(name, value, member as FieldInfo, att, context);
                    return (true, obj);
                }
            }

            var label = s_Temp;
            label.text = name;
            label.tooltip = null;
            label.image = null;

            if (attributes.FirstOrDefault(a => a is TooltipAttribute) is TooltipAttribute tooltipAttribute)
            {
                label.tooltip = tooltipAttribute.tooltip;
            }

            //Then check UnityObjects
            if (typeof(UObject).IsAssignableFrom(t))
            {
                //if (t == typeof(Component) && (Component)value != null)
                //{
                //    //return ComponentField(label, (Component)value, typeof(Component));
                //}

                var obj = EditorGUILayout.ObjectField(label, (UObject)value, t, typeof(Component).IsAssignableFrom(t) || t == typeof(GameObject));
                return (true, obj);
            }

            ////Force UnityObject field?
            //if (member != null && attributes.Any(a => a is ForceObjectFieldAttribute))
            //{
            //    return EditorGUILayout.ObjectField(label, value as UnityObject, t, typeof(Component).IsAssignableFrom(t) || t == typeof(GameObject));
            //}

            //Restricted popup values?
            if (member != null)
            {
                //var popAtt = attributes.FirstOrDefault(a => a is PopupFieldAttribute) as PopupFieldAttribute;
                //if (popAtt != null)
                //{
                //    if (popAtt.staticPath != null)
                //    {
                //        try
                //        {
                //            var typeName = popAtt.staticPath.Substring(0, popAtt.staticPath.LastIndexOf("."));
                //            var type = ReflectionTools.GetType(typeName, /*fallback?*/false);
                //            var start = popAtt.staticPath.LastIndexOf(".") + 1;
                //            var end = popAtt.staticPath.Length;
                //            var propName = popAtt.staticPath.Substring(start, end - start);
                //            var prop = type.GetProperty(propName, BindingFlags.Static | BindingFlags.Public);
                //            var propValue = prop.GetValue(null, null);
                //            var values = ((IEnumerable)propValue).Cast<object>().ToList();
                //            return Popup<object>(label, value, values);
                //        }
                //        catch
                //        {
                //            EditorGUILayout.LabelField(label, "[PopupField] attribute error!");
                //            return value;
                //        }
                //    }
                //    return Popup<object>(label, value, popAtt.values.ToList());
                //}
            }

            //Check Type of Type
            if (t == typeof(Type))
            {
                //return Popup<Type>(label, (Type)value, UserTypePrefs.GetPreferedTypesList(typeof(object), true));
            }

            //Check abstract
            if (!drawReference)
            {
                if ((value != null && value.GetType().IsAbstract) || (value == null && t.IsAbstract))
                {
                    EditorGUILayout.LabelField(label, string.Format("abstract ({0})", t.Name));
                    return (false, value);
                }
            }

            //Create instance for some types
            if (value == null && !t.IsAbstract && !t.IsInterface && (t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null || t.IsArray))
            {
                if (t.IsArray)
                {
                    value = Array.CreateInstance(t.GetElementType(), 0);
                }
                else
                {
                    value = Activator.CreateInstance(t);
                }
            }

            //Check the rest
            //..............
            if (t == typeof(string))
            {
                if (member != null)
                {
                    //if (attributes.Any(a => a is TagFieldAttribute))
                    //    return EditorGUILayout.TagField(label, (string)value);
                    var areaAtt = attributes.FirstOrDefault(a => a is TextAreaAttribute) as TextAreaAttribute;
                    if (areaAtt != null)
                    {
                        GUILayout.Label(label);
                        var areaStyle = new GUIStyle(GUI.skin.GetStyle("TextArea"));
                        areaStyle.wordWrap = true;
                        var s = EditorGUILayout.TextArea((string)value, areaStyle, GUILayout.Height(areaAtt.minLines * EditorGUIUtility.singleLineHeight));
                        return (true, s);
                    }
                }

                var obj = EditorGUILayout.TextField(label, (string)value);
                return (true, obj);
            }

            if (t == typeof(bool))
            {
                var obj = EditorGUILayout.Toggle(label, (bool)value);
                return (true, obj);
            }

            if (t == typeof(int))
            {
                if (member != null)
                {
                    //var sField = attributes.FirstOrDefault(a => a is SliderFieldAttribute) as SliderFieldAttribute;
                    //if (sField != null)
                    //    return (int)EditorGUILayout.Slider(label, (int)value, (int)sField.left, (int)sField.right);
                    //if (attributes.Any(a => a is LayerFieldAttribute))
                    //    return EditorGUILayout.LayerField(label, (int)value);
                }

                var obj = EditorGUILayout.IntField(label, (int)value);
                return (true, obj);
            }

            if (t == typeof(float))
            {
                if (member != null)
                {
                    //var sField = attributes.FirstOrDefault(a => a is SliderFieldAttribute) as SliderFieldAttribute;
                    //if (sField != null)
                    //    return EditorGUILayout.Slider(label, (float)value, sField.left, sField.right);
                }
                var obj = EditorGUILayout.FloatField(label, (float)value);
                return (true, obj);
            }

            if (t == typeof(byte))
            {
                var obj = Convert.ToByte(Mathf.Clamp(EditorGUILayout.IntField(label, (byte)value), 0, 255));
                return (true, obj);
            }

            if (t == typeof(Vector2))
            {
                var obj = EditorGUILayout.Vector2Field(label, (Vector2)value);
                return (true, obj);
            }

            if (t == typeof(Vector3))
            {
                var obj = EditorGUILayout.Vector3Field(label, (Vector3)value);
                return (true, obj);
            }

            if (t == typeof(Vector4))
            {
                var obj = EditorGUILayout.Vector4Field(label, (Vector4)value);
                return (true, obj);
            }

            if (t == typeof(Quaternion))
            {
                var quat = (Quaternion)value;
                var vec3 = EditorGUILayout.Vector3Field(label, quat.eulerAngles);
                var obj = Quaternion.Euler(vec3.x, vec3.y, vec3.z);
                return (true, obj);
            }

            if (t == typeof(Color))
            {
                var obj = EditorGUILayout.ColorField(label, (Color)value);
                return (true, obj);
            }

            if (t == typeof(Rect))
            {
                var obj = EditorGUILayout.RectField(label, (Rect)value);
                return (true, obj);
            }

            if (t == typeof(AnimationCurve))
            {
                var obj = EditorGUILayout.CurveField(label, (AnimationCurve)value);
                return (true, obj);
            }

            if (t == typeof(Bounds))
            {
                var obj = EditorGUILayout.BoundsField(label, (Bounds)value);
                return (true, obj);
            }

            if (t == typeof(LayerMask))
            {
                var obj = EditorGUILayout.LayerField(label, (LayerMask)value);
                return (true, obj);
            }

            #region unity mathematics

            if (t == typeof(float2))
            {
                var obj = (float2)EditorGUILayout.Vector2Field(label, (float2)value);
                return (true, obj);
            }

            if (t == typeof(float3))
            {
                var obj = (float3)EditorGUILayout.Vector3Field(label, (float3)value);
                return (true, obj);
            }

            if (t == typeof(float4))
            {
                var obj = (float4)EditorGUILayout.Vector4Field(label, (float4)value);
                return (true, obj);
            }

            if (t == typeof(quaternion))
            {
                var quat = (Quaternion)(quaternion)value;
                var vec3 = EditorGUILayout.Vector3Field(label, quat.eulerAngles);
                var obj = quaternion.EulerXYZ(math.radians(vec3));
                return (true, obj);
            }

            #endregion

            #region fixed math
#if FIXED_POINT_MATH
            if (t == typeof(sfloat))
            {
                var obj = (sfloat)EditorGUILayout.FloatField(label, (float)(sfloat)value);
                return (true, obj);
            }

            if (t == typeof(sfloat2))
            {
                var obj = (sfloat2)EditorGUILayout.Vector2Field(label, (Vector2)(sfloat2)value);
                return (true, obj);
            }

            if (t == typeof(sfloat3))
            {
                var obj = (sfloat3)EditorGUILayout.Vector3Field(label, (Vector3)(sfloat3)value);
                return (true, obj);
            }

            if (t == typeof(sfloat4))
            {
                var obj = (sfloat4)EditorGUILayout.Vector4Field(label, (Vector4)(sfloat4)value);
                return (true, obj);
            }

            if (t == typeof(squaternion))
            {
                var quat = (Quaternion)(squaternion)value;
                var vec3 = EditorGUILayout.Vector3Field(label, quat.eulerAngles);
                var obj = squaternion.EulerXYZ(math.radians(vec3));
                return (true, obj);
            }
#endif
            #endregion

            if (t.IsSubclassOf(typeof(Enum)))
            {
                var obj = EditorGUILayout.EnumPopup(label, (System.Enum)value);
                return (true, obj);
            }

            if (typeof(IList).IsAssignableFrom(t))
            {
                var obj = ListEditor(name, (IList)value);
                return (true, obj);
            }

            if (typeof(IDictionary).IsAssignableFrom(t))
            {
                var obj = DictionaryEditor(name, (IDictionary)value, t);
                return (true, obj);
            }

            GUILayout.BeginVertical("box");

            if (drawReference)
            {
                var rect = EditorGUILayout.GetControlRect();
                DrawSerializeReference(rect, t, value, member as FieldInfo, context);

                //show nested class members recursively
                if (value != null && !t.IsEnum && !t.IsInterface)
                {
                    ShowSubObject(rect, value, name);
                }
            }
            else
            {
                if (value != null && !t.IsEnum && !t.IsInterface)
                {
                    ShowSubObject(EditorGUILayout.GetControlRect(), value, name);
                }
                else
                    EditorGUILayout.LabelField(name, $"unhandled type: {t.Name}");
            }

            GUILayout.EndVertical();

            return (false, value);
        }

        private static void ShowSubObject(Rect position, object value, string name, bool defaultFoldout = true)
        {
            //register foldout
            if (!k_RegisteredEditorFoldouts.ContainsKey(value))
                k_RegisteredEditorFoldouts[value] = defaultFoldout;

            var foldout = k_RegisteredEditorFoldouts[value];
            foldout = EditorGUI.Foldout(position, foldout, $"{name} : {value.GetType().Name}", true/*, EditorStyles.boldLabel*/);
            k_RegisteredEditorFoldouts[value] = foldout;

            if (foldout)
            {
                EditorGUI.indentLevel++;
                ShowAutoEditorGUI(value);
                EditorGUI.indentLevel--;
            }
        }

        private static readonly Dictionary<object, bool> k_RegisteredEditorFoldouts = new();

        public static void SetEditorFoldout(object o, bool foldout)
        {
            k_RegisteredEditorFoldouts[o] = foldout;
        }

        public static bool GetEditorFoldout(object o)
        {
            if (k_RegisteredEditorFoldouts.TryGetValue(o, out var foldout))
                return foldout;
            return true; // default is true
        }

        //An IList editor (List<T> and Arrays)
        public static IList ListEditor(string prefix, IList list)
        {
            var listType = list.GetType();
            var argType = listType.IsArray ? listType.GetElementType() : listType.GetGenericArguments()[0];

            //register foldout
            if (!k_RegisteredEditorFoldouts.ContainsKey(list))
                k_RegisteredEditorFoldouts[list] = false;

            GUILayout.BeginVertical();

            if (!string.IsNullOrEmpty(prefix))
            {
                var foldout = k_RegisteredEditorFoldouts[list];
                foldout = EditorGUILayout.Foldout(foldout, prefix, true);
                k_RegisteredEditorFoldouts[list] = foldout;

                if (!foldout)
                {
                    GUILayout.EndVertical();
                    return list;
                }
            }

            if (list == null)
            {
                GUILayout.Label("Null List");
                GUILayout.EndVertical();
                return list;
            }

            if (GUILayout.Button("Add Element"))
            {
                if (listType.IsArray)
                {
                    list = ResizeArray((Array)list, list.Count + 1);
                    k_RegisteredEditorFoldouts[list] = true;
                }
                else
                {
                    var o = argType.IsValueType ? Activator.CreateInstance(argType) : null;
                    list.Add(o);
                }
            }

            EditorGUI.indentLevel++;

            for (var i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal("box");
                var result = GenericField("Element " + i, list[i], argType, null);
                //if (result.draw)
                list[i] = result.obj;

                if (GUILayout.Button("X", GUILayout.Width(18)))
                {
                    if (listType.IsArray)
                    {
                        list = ResizeArray((Array)list, list.Count - 1);
                        k_RegisteredEditorFoldouts[list] = true;
                    }
                    else
                    {
                        list.RemoveAt(i);
                    }
                }
                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
            Separator();

            GUILayout.EndVertical();
            return list;
        }

        //A dictionary editor
        public static IDictionary DictionaryEditor(string prefix, IDictionary dict, Type dictType)
        {
            var keyType = dictType.GetGenericArguments()[0];
            var valueType = dictType.GetGenericArguments()[1];

            //register foldout
            if (!k_RegisteredEditorFoldouts.ContainsKey(dict))
                k_RegisteredEditorFoldouts[dict] = false;

            GUILayout.BeginVertical();

            var foldout = k_RegisteredEditorFoldouts[dict];
            foldout = EditorGUILayout.Foldout(foldout, prefix);
            k_RegisteredEditorFoldouts[dict] = foldout;

            if (!foldout)
            {
                GUILayout.EndVertical();
                return dict;
            }

            if (dict.Equals(null))
            {
                GUILayout.Label("Null Dictionary");
                GUILayout.EndVertical();
                return dict;
            }

            var keys = dict.Keys.Cast<object>().ToList();
            var values = dict.Values.Cast<object>().ToList();

            if (GUILayout.Button("Add Element"))
            {
                if (!typeof(UObject).IsAssignableFrom(keyType))
                {
                    object newKey = null;
                    if (keyType == typeof(string))
                        newKey = string.Empty;
                    else newKey = Activator.CreateInstance(keyType);
                    if (dict.Contains(newKey))
                    {
                        Debug.LogWarning(string.Format("Key '{0}' already exists in Dictionary", newKey.ToString()));
                        return dict;
                    }

                    keys.Add(newKey);
                }
                else
                {
                    Debug.LogWarning("Can't add a 'null' Dictionary Key");
                    return dict;
                }

                values.Add(valueType.IsValueType ? Activator.CreateInstance(valueType) : null);
            }

            //clear before reconstruct
            dict.Clear();

            for (var i = 0; i < keys.Count; i++)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Box("", GUILayout.Width(6), GUILayout.Height(35));
                GUILayout.BeginVertical();

                var resultKey = GenericField("K:", keys[i], keyType, null);
                //if (resultKey.draw)
                keys[i] = resultKey.obj;
                var resultValue = GenericField("V:", values[i], valueType, null);
                //if (resultValue.draw)
                values[i] = resultValue.obj;

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                try
                {
                    dict.Add(keys[i], values[i]);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{keys[i]}: {values[i]} \n{e}");
                }
            }

            Separator();

            GUILayout.EndVertical();
            return dict;
        }

        private static readonly Dictionary<Type, List<Type>> k_ReferenceCache = new();

        public static void DrawSerializeReference(Rect position, Type t, object value, FieldInfo field, object context)
        {
            if (!k_ReferenceCache.TryGetValue(t, out var subTypes))
            {
                subTypes = TypeUtility.GetSubClassTypesAllAssemblies(t, false);

                k_ReferenceCache.Add(t, subTypes);
            }

            var labelRect = position;
            EditorGUI.LabelField(labelRect, value == null ? t.Name : "");

            var buttonRect = position;
            buttonRect.width = 18f;
            buttonRect.x += position.width - buttonRect.width;
            if (GUI.Button(buttonRect, "+"))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Null"), false, () =>
                {
                    field.SetValue(context, null);
                });
                foreach (var subType in subTypes)
                {
                    if (subType.ContainsGenericParameters) continue;

                    menu.AddItem(new GUIContent(subType.Name), false, () =>
                    {
                        value = Activator.CreateInstance(subType);
                        field.SetValue(context, value);
                    });
                }
                menu.ShowAsContext();
            }
        }

        static System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);
            if (preserveLength > 0)
            {
                System.Array.Copy(oldArray, newArray, preserveLength);
            }
            return newArray;
        }

        //a thin separator
        public static void Separator()
        {
            GUI.backgroundColor = Color.black;
            GUILayout.Box("", GUILayout.MaxWidth(Screen.width), GUILayout.Height(2));
            GUI.backgroundColor = Color.white;
        }

        [Conditional("UNITY_EDITOR")]
        public static void SetDirty(UObject target)
        {
#if UNITY_EDITOR
            if (Application.isPlaying || target == null) { return; }
            EditorUtility.SetDirty(target);
#endif
        }

        public static Texture2D GetIcon(string icon)
        {
            return Resources.Load<Texture2D>("icons/" + icon);
        }

        public static void OpenScriptByType(Type type)
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(MonoScript)} {type.Name}");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript.GetClass() == type)
                {
                    AssetDatabase.OpenAsset(monoScript);
                }
            }
        }
    }
}

#endif
