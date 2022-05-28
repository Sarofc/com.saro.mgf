#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Saro.Core.Editor
{
    /*
     * TODO 像 GameplayTag 那样搞个 搜索下拉框？
     */
    [CustomPropertyDrawer(typeof(AssetTableIDAttribute))]
    internal sealed class AssetTableIDDrawer : PropertyDrawer
    {
        private const int k_PathPreviewHeight = 18;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var AssetIDAttribute = (AssetTableIDAttribute)attribute;
            if (AssetIDAttribute.ShowPathPreview)
            {
                position.height -= k_PathPreviewHeight;
            }

            if (!IsPropertyTypeValid(property))
            {
                position = EditorGUI.PrefixLabel(position, label);
                EditorGUI.LabelField(position,
                    $"Error: {typeof(AssetTableIDAttribute)} attribute can be applied only to {SerializedPropertyType.String} type");
                return;
            }

            var assetID = property.intValue;

            var assetPath = EditorAssetTableGetter.GetAssetTable().GetAssetPath(assetID);
            Object asset = null;
            if (!string.IsNullOrEmpty(assetPath))
            {
                asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
            }

            var color = GUI.color;
            if (assetID != 0 && asset == null) GUI.color = Color.red;
            EditorGUI.PropertyField(position, property);
            GUI.color = color;

            if (AssetIDAttribute.ShowPathPreview)
            {
                position.y += k_PathPreviewHeight;
                position = EditorGUI.PrefixLabel(position, new GUIContent("\t"));

                EditorGUI.BeginChangeCheck();
                asset = EditorGUI.ObjectField(position, asset, typeof(Object), false);
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
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsPropertyTypeValid(property) && ((AssetTableIDAttribute)attribute).ShowPathPreview)
            {
                return base.GetPropertyHeight(property, label) + k_PathPreviewHeight;
            }

            return base.GetPropertyHeight(property, label);
        }

        private bool IsPropertyTypeValid(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Integer;
        }
    }
}
#endif