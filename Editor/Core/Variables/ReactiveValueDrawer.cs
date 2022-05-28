
namespace Saro
{
    // forked from UniRx

    using System;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(IntReactiveValue))]
    [CustomPropertyDrawer(typeof(FloatReactiveValue))]
    [CustomPropertyDrawer(typeof(ByteReactiveValue))]
    [CustomPropertyDrawer(typeof(LongReactiveValue))]
    [CustomPropertyDrawer(typeof(DoubleReactiveValue))]
    [CustomPropertyDrawer(typeof(BoolReactiveValue))]
    [CustomPropertyDrawer(typeof(Vector2ReactiveValue))]
    [CustomPropertyDrawer(typeof(Vector3ReactiveValue))]
    [CustomPropertyDrawer(typeof(QuaternionReactiveValue))]
    [CustomPropertyDrawer(typeof(RectReactiveValue))]
    public class ReactiveValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var value = property.FindPropertyRelative("m_Value");
            EditorGUI.PropertyField(position, value, label, true);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties(); // deserialize to field
                var paths = property.propertyPath.Split('.'); // X.Y.Z...
                var attachedComponent = property.serializedObject.targetObject;

                var targetProp = (paths.Length == 1)
                    ? fieldInfo.GetValue(attachedComponent)
                    : GetValueRecursive(attachedComponent, 0, paths);
                if (targetProp == null) return;
                var propInfo = targetProp.GetType().GetProperty("value", BindingFlags.IgnoreCase | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var modifiedValue = propInfo.GetValue(targetProp, null); // retrieve new value

                var methodInfo = targetProp.GetType().GetMethod("OnValueChanged", BindingFlags.IgnoreCase | BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (methodInfo != null)
                {
                    methodInfo.Invoke(targetProp, new object[] { modifiedValue });
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);
            var valueProperty = property.FindPropertyRelative("m_Value");
            if (valueProperty == null)
            {
                return height;
            }

            if (valueProperty.propertyType == SerializedPropertyType.Rect)
            {
                return height * 2;
            }
            if (valueProperty.propertyType == SerializedPropertyType.Bounds)
            {
                return height * 3;
            }
            //if (valueProperty.propertyType == SerializedPropertyType.String)
            //{
            //    var multilineAttr = GetMultilineAttribute();
            //    if (multilineAttr != null)
            //    {
            //        return ((!EditorGUIUtility.wideMode) ? 16f : 0f) + 16f + (float)((multilineAttr.Lines - 1) * 13);
            //    };
            //}

            if (valueProperty.isExpanded)
            {
                var count = 0;
                var e = valueProperty.GetEnumerator();
                while (e.MoveNext()) count++;
                return ((height + 4) * count) + 6; // (Line = 20 + Padding) ?
            }

            return height;
        }

        private object GetValueRecursive(object obj, int index, string[] paths)
        {
            var path = paths[index];

            FieldInfo fldInfo = null;
            var type = obj.GetType();
            while (fldInfo == null)
            {
                // attempt to get information about the field
                fldInfo = type.GetField(path, BindingFlags.IgnoreCase | BindingFlags.GetField | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (fldInfo != null ||
                    type.BaseType == null ||
                    type.BaseType.IsSubclassOf(typeof(ReactiveValue<>))) break;

                // if the field information is missing, it may be in the base class
                type = type.BaseType;
            }

            // If array, path = Array.data[index]
            if (fldInfo == null && path == "Array")
            {
                try
                {
                    path = paths[++index];
                    var m = Regex.Match(path, @"(.+)\[([0-9]+)*\]");
                    var arrayIndex = int.Parse(m.Groups[2].Value);
                    var arrayValue = (obj as System.Collections.IList)[arrayIndex];
                    if (index < paths.Length - 1)
                    {
                        return GetValueRecursive(arrayValue, ++index, paths);
                    }
                    else
                    {
                        return arrayValue;
                    }
                }
                catch
                {
                    throw new Exception("InspectorDisplayDrawer Exception, objType:" + obj.GetType().Name + " path:" + string.Join(", ", paths));
                }
            }
            else if (fldInfo == null)
            {
                throw new Exception("Can't decode path, please report to UniRx's GitHub issues:" + string.Join(", ", paths));
            }

            var v = fldInfo.GetValue(obj);
            if (index < paths.Length - 1)
            {
                return GetValueRecursive(v, ++index, paths);
            }

            return v;
        }
    }

}