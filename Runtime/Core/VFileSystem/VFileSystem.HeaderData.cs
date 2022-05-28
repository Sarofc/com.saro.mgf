using System.Runtime.InteropServices;

namespace Saro.IO
{
    public sealed partial class VFileSystem
    {
        /// <summary>
        /// 数据头
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct HeaderData
        {
            private const int k_HeaderLength = 3;
            private const int k_EncryptBytesLength = 4;
            private static readonly byte[] s_Header = new byte[k_HeaderLength] { (byte)'G', (byte)'A', (byte)'F' }; // TODO 整一个编码

            /// <summary>
            /// 虚拟文件编码
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = k_HeaderLength)]
            private readonly byte[] m_Header;

            private readonly byte m_Version;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = k_EncryptBytesLength)]
            private readonly byte[] m_EncryptBytes;

            private readonly int m_MaxFileCount;
            private readonly int m_MaxBlockCount;
            private readonly int m_BlockCount;

            // 20 = 3 + 1 + 4 + 4 + 4 + 4

            /// <summary>
            /// 虚拟文件版本
            /// </summary>
            public byte Version => m_Version;

            /// <summary>
            /// 最大文件数量
            /// </summary>
            public int MaxFileCount => m_MaxFileCount;

            /// <summary>
            /// 最大数据块数量
            /// </summary>
            public int MaxBlockCount => m_MaxBlockCount;

            /// <summary>
            /// 数据块数量
            /// </summary>
            public int BlockCount => m_BlockCount;

            public HeaderData(int maxFileCount, int maxBlockCount)
                : this(0, new byte[k_EncryptBytesLength], maxFileCount, maxBlockCount, 0)
            { }

            public HeaderData(byte version, byte[] encryptBytes, int maxFileCount, int maxBlockCount, int blockCount)
            {
                m_Header = s_Header;
                m_Version = version;
                m_EncryptBytes = encryptBytes;
                m_MaxFileCount = maxFileCount;
                m_MaxBlockCount = maxBlockCount;
                m_BlockCount = blockCount;
            }

            /// <summary>
            /// 虚拟文件是否合法
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return m_Header.Length == k_HeaderLength
                    && m_Header[0] == s_Header[0]
                    && m_Header[1] == s_Header[1]
                    && m_Header[2] == s_Header[2]
                    && m_Version == 0
                    && m_EncryptBytes.Length == k_EncryptBytesLength
                    && m_MaxFileCount > 0
                    && m_MaxBlockCount > 0
                    && m_MaxFileCount <= m_MaxBlockCount
                    && m_BlockCount > 0
                    && m_BlockCount <= m_MaxBlockCount;
            }

            /// <summary>
            /// 加密key
            /// </summary>
            /// <returns></returns>
            internal byte[] GetEncryptBytes() => m_EncryptBytes;

            /// <summary>
            /// 设置数据块数量
            /// </summary>
            /// <param name="blockCount"></param>
            /// <returns></returns>
            internal HeaderData SetBlockCount(int blockCount)
                => new(m_Version, m_EncryptBytes, m_MaxFileCount, m_MaxBlockCount, blockCount);

            public override string ToString()
                => $"header: {System.Text.Encoding.UTF8.GetString(m_Header)} version: {m_Version}";
        }
    }
}
