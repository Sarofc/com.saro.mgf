using System;
using System.Diagnostics;
using UnityEngine;

namespace Saro.SEditor
{
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