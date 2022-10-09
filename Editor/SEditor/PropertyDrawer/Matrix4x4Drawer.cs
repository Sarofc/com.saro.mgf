#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Saro.SEditor
{
    [CustomPropertyDrawer(typeof(Matrix4x4))]
    public class Matrix4x4Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float columnWidth;
            columnWidth = position.width / 4;

            var rowHeight = 16;

            // Calculate rects
            var m00Rect = new Rect(position.x + 0 * columnWidth, position.y, columnWidth, rowHeight);
            var m01Rect = new Rect(position.x + 1 * columnWidth, position.y, columnWidth, rowHeight);
            var m02Rect = new Rect(position.x + 2 * columnWidth, position.y, columnWidth, rowHeight);

            var m10Rect = new Rect(position.x + 0 * columnWidth, position.y + rowHeight, columnWidth, rowHeight);
            var m11Rect = new Rect(position.x + 1 * columnWidth, position.y + rowHeight, columnWidth, rowHeight);
            var m12Rect = new Rect(position.x + 2 * columnWidth, position.y + rowHeight, columnWidth, rowHeight);

            var m20Rect = new Rect(position.x + 0 * columnWidth, position.y + rowHeight * 2, columnWidth, rowHeight);
            var m21Rect = new Rect(position.x + 1 * columnWidth, position.y + rowHeight * 2, columnWidth, rowHeight);
            var m22Rect = new Rect(position.x + 2 * columnWidth, position.y + rowHeight * 2, columnWidth, rowHeight);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(m00Rect, property.FindPropertyRelative("e00"), GUIContent.none);
            EditorGUI.PropertyField(m01Rect, property.FindPropertyRelative("e01"), GUIContent.none);
            EditorGUI.PropertyField(m02Rect, property.FindPropertyRelative("e02"), GUIContent.none);

            EditorGUI.PropertyField(m10Rect, property.FindPropertyRelative("e10"), GUIContent.none);
            EditorGUI.PropertyField(m11Rect, property.FindPropertyRelative("e11"), GUIContent.none);
            EditorGUI.PropertyField(m12Rect, property.FindPropertyRelative("e12"), GUIContent.none);

            EditorGUI.PropertyField(m20Rect, property.FindPropertyRelative("e20"), GUIContent.none);
            EditorGUI.PropertyField(m21Rect, property.FindPropertyRelative("e21"), GUIContent.none);
            EditorGUI.PropertyField(m22Rect, property.FindPropertyRelative("e22"), GUIContent.none);

            var m03Rect = new Rect(position.x + 3 * columnWidth, position.y, columnWidth, rowHeight);
            var m13Rect = new Rect(position.x + 3 * columnWidth, position.y + rowHeight, columnWidth, rowHeight);
            var m23Rect = new Rect(position.x + 3 * columnWidth, position.y + rowHeight * 2, columnWidth, rowHeight);

            var m30Rect = new Rect(position.x + 0 * columnWidth, position.y + rowHeight * 3, columnWidth, rowHeight);
            var m31Rect = new Rect(position.x + 1 * columnWidth, position.y + rowHeight * 3, columnWidth, rowHeight);
            var m32Rect = new Rect(position.x + 2 * columnWidth, position.y + rowHeight * 3, columnWidth, rowHeight);
            var m33Rect = new Rect(position.x + 3 * columnWidth, position.y + rowHeight * 3, columnWidth, rowHeight);

            EditorGUI.PropertyField(m03Rect, property.FindPropertyRelative("e03"), GUIContent.none);
            EditorGUI.PropertyField(m13Rect, property.FindPropertyRelative("e13"), GUIContent.none);
            EditorGUI.PropertyField(m23Rect, property.FindPropertyRelative("e23"), GUIContent.none);

            EditorGUI.PropertyField(m30Rect, property.FindPropertyRelative("e30"), GUIContent.none);
            EditorGUI.PropertyField(m31Rect, property.FindPropertyRelative("e31"), GUIContent.none);
            EditorGUI.PropertyField(m32Rect, property.FindPropertyRelative("e32"), GUIContent.none);
            EditorGUI.PropertyField(m33Rect, property.FindPropertyRelative("e33"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Gets the height of the property.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 64;
        }
    }
}
#endif