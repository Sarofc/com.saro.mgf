#if UNITY_EDITOR
using System;

namespace Saro.Core
{
    public static class EditorAssetTableGetter
    {
        /// <summary>
        /// AssetTable is AutoCollect?
        /// </summary>
        public static Func<bool> AutoCollect { get; set; } = () => true;

        /// <summary>
        /// Get IAssetTable instance
        /// <code>override this for your AssetTable workflow</code>
        /// </summary>
        public static Func<IAssetTable> GetAssetTable { get; set; } = AssetTableSO.GetTable;
    }
}
#endif