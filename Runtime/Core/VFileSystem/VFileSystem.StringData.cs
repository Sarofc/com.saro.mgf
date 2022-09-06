using System;
using System.Runtime.InteropServices;

namespace Saro.IO
{
    public sealed partial class VFileSystem
    {
        /// <summary>
        /// 字符串数据
        /// <code>warn: utf-8 with bom</code>
        /// <code>warn: 字符串最大只支持 255byte ?</code>
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct StringData
        {
            private static readonly byte[] s_CachedBytes = new byte[byte.MaxValue + 1]; // why +1 ?

            private readonly byte m_Length;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = byte.MaxValue)]
            private readonly byte[] m_Bytes;

            // 256 = 1 + 255

            public StringData(byte length, byte[] bytes)
            {
                m_Length = length;
                m_Bytes = bytes;
            }

            public string GetString(byte[] encryptBytes)
            {
                if (m_Length <= 0) return null;

                Array.Copy(m_Bytes, 0, s_CachedBytes, 0, m_Length);

                Utility.EncryptionUtility.SelfXorBytes(s_CachedBytes, encryptBytes, 0, m_Length);
                return System.Text.Encoding.UTF8.GetString(s_CachedBytes, 0, m_Length);
            }

            public StringData SetString(string val, byte[] encryptBytes)
            {
                if (string.IsNullOrEmpty(val)) return Clear();

                var length = System.Text.Encoding.UTF8.GetBytes(val, 0, val.Length, s_CachedBytes, 0);
                if (length > byte.MaxValue)
                {
                    throw new Exception($"string '{val}' is too long.");
                }
                Utility.EncryptionUtility.SelfXorBytes(s_CachedBytes, encryptBytes, 0, m_Length);
                Array.Copy(s_CachedBytes, 0, m_Bytes, 0, length);
                return new StringData((byte)length, m_Bytes);
            }

            public StringData Clear() => new(0, m_Bytes);
        }
    }
}
