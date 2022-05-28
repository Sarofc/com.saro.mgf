using System;
using UnityEngine;

namespace Saro.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetTableIDAttribute : PropertyAttribute
    {
        public bool ShowPathPreview { get; }

        public AssetTableIDAttribute(bool showPathPreview = false)
        {
            ShowPathPreview = showPathPreview;
        }
    }
}