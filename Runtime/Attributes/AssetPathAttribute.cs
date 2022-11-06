namespace Saro.SEditor
{
    using System;
    using System.Diagnostics;
    using UnityEngine;

    /// <summary>
    /// An attribute that can be placed on a string field to make it appear in
    /// the inspector as an object picker for the specified type and the
    /// selected objects path will be saved to the string field.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AssetPathAttribute : PropertyAttribute
    {
        /// <summary>
        /// The type of the asset this field points to.
        /// </summary>
        public Type AssetType { get; private set; }

        /// Show asset path preview in the PropertyDrawer, not just the ObjectField.
        /// </summary>
        public bool ShowPathPreview { get; private set; }

        /// <summary>
        /// Flags a field to be inspected as an object picker where only the
        /// path is saved.
        /// </summary>
        /// <param name="assetType">The type of the asset this field points to.</param>
        /// <param name="showPathPreview">
        /// Is the asset supposed to be in the resources directory? If so the
        /// path will automatically be made relative to the resources directory
        /// and the extension will be removed.
        /// </param>
        public AssetPathAttribute(Type assetType, bool showPathPreview = false)
        {
            AssetType = assetType;
            ShowPathPreview = showPathPreview;
        }
    }
}

#if UNITY_EDITOR

namespace Saro.SEditor
{
    using UnityEditor;
    using UnityEngine;

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