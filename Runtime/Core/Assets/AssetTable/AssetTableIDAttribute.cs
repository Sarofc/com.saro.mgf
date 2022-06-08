using System;
using UnityEngine;

namespace Saro.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetTableIDAttribute : PropertyAttribute
    {
        public bool ShowObjectPreview { get; }

        public AssetTableIDAttribute(bool showObjectPreview = false)
        {
            ShowObjectPreview = showObjectPreview;
        }
    }
}