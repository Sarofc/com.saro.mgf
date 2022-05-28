#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Saro.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(AssetPathAttribute))]
    internal sealed class AssetPathDrawer : PropertyDrawer
    {
        private const int k_PathPreviewHeight = 16;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var assetPathAttribute = (AssetPathAttribute)attribute;
            if (assetPathAttribute.ShowPathPreview)
            {
                position.height -= k_PathPreviewHeight;
            }

            if (!IsPropertyTypeValid(property))
            {
                position = EditorGUI.PrefixLabel(position, label);
                EditorGUI.LabelField(position, string.Format("Error: {0} attribute can be applied only to {1} type", typeof(AssetPathAttribute), SerializedPropertyType.String));
                return;
            }

            var assetPath = property.stringValue;
            UnityEngine.Object asset = null;
            if (!string.IsNullOrEmpty(assetPath))
            {
                asset = AssetDatabase.LoadAssetAtPath(assetPath, assetPathAttribute.AssetType);
            }

            EditorGUI.BeginChangeCheck();
            asset = EditorGUI.ObjectField(position, "[path]" + label.text, asset, assetPathAttribute.AssetType, false);
            if (EditorGUI.EndChangeCheck())
            {
                if (asset == null)
                {
                    property.stringValue = null;
                }
                else
                {
                    assetPath = AssetDatabase.GetAssetPath(asset);

                    property.stringValue = assetPath;
                }
            }

            if (assetPathAttribute.ShowPathPreview)
            {
                position.y += k_PathPreviewHeight;
                position = EditorGUI.PrefixLabel(position, new GUIContent("  Asset Path Preview"));
                EditorGUI.LabelField(position, string.Format("\"{0}\"", assetPath));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsPropertyTypeValid(property) && ((AssetPathAttribute)attribute).ShowPathPreview)
            {
                return base.GetPropertyHeight(property, label) + k_PathPreviewHeight;
            }
            else
            {
                return base.GetPropertyHeight(property, label);
            }
        }

        private bool IsPropertyTypeValid(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.String;
        }
    }
}
#endif