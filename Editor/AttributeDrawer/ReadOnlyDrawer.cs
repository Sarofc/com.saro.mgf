#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Saro.SEditor
{

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal sealed class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enalbe = GUI.enabled;
            if (enalbe)
            {
                GUI.enabled = false;

                EditorGUI.PropertyField(position, property, label, true);

                GUI.enabled = true;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, property.isExpanded);
        }
    }
}


#endif