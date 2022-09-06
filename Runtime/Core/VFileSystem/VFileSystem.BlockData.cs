using System.Runtime.InteropServices;

namespace Saro.IO
{
    public sealed partial class VFileSystem
    {
        /// <summary>
        /// 数据块
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct BlockData
        {
            public static readonly BlockData s_Empty = new BlockData(0, 0);

            private readonly int m_StringIndex;
            private readonly int m_ClusterIndex;
            private readonly int m_Length;

            // 12 = 4 + 4 + 4

            public BlockData(int clusterIndex, int length) : this(-1, clusterIndex, length)
            { }

            public BlockData(int stringIndex, int clusterIndex, int length)
            {
                m_StringIndex = stringIndex;
                m_ClusterIndex = clusterIndex;
                m_Length = length;
            }

            /// <summary>
            /// 被使用
            /// </summary>
            public bool Using => m_StringIndex >= 0;

            /// <summary>
            /// 字符数据串索引
            /// </summary>
            public int StringIndex => m_StringIndex;

            /// <summary>
            /// 簇索引
            /// </summary>
            public int ClusterIndex => m_ClusterIndex;

            /// <summary>
            /// 数据长度
            /// </summary>
            public int Length => m_Length;

            public BlockData Free() => new(m_ClusterIndex, (int)GetUpBoundClusterOffset(m_Length));
        }
    }
}
