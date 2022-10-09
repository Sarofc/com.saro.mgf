using System;
using System.Linq.Expressions;
using UnityEditor;

namespace Saro.SEditor
{
    public static class SerializedPropertyUtility
    {
        /// Usage: instead of
        /// <example>
        /// SerializedPropertyUtility.FindProperty("m_MyField");
        /// </example>
        /// do this:
        /// <example>
        /// MyClass myclass = null;
        /// mySerializedObject.FindProperty( () => myClass.m_MyField);
        /// </example>
        public static SerializedProperty FindProperty<TValue>(this SerializedObject obj, Expression<Func<TValue>> exp)
        {
            return obj.FindProperty(Utility.TypeUtility.PropertyName(exp));
        }

        /// Usage: instead of
        /// <example>
        /// mySerializedProperty.FindPropertyRelative("m_MyField");
        /// </example>
        /// do this:
        /// <example>
        /// MyClass myclass = null;
        /// mySerializedProperty.FindPropertyRelative( () => myClass.m_MyField);
        /// </example>
        public static SerializedProperty FindPropertyRelative<TValue>(this SerializedProperty obj, Expression<Func<TValue>> exp)
        {
            return obj.FindPropertyRelative(Utility.TypeUtility.PropertyName(exp));
        }

        /// <summary>Get the value of a property, as an object</summary>
        public static object GetPropertyValue(SerializedProperty property)
        {
            var targetObject = property.serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();
            var field = targetObjectClassType.GetField(property.propertyPath);
            if (field != null)
                return field.GetValue(targetObject);
            return null;
        }
    }
}