using System;

namespace Saro.Utility
{
    public static class EncryptionUtility
    {
        public const int k_QuickEncryptLength = 220;

        public static void QuickSelfXorBytes(byte[] bytes, byte[] key)
        {
            SelfXorBytes(bytes, key, 0, Math.Min(k_QuickEncryptLength, bytes.Length));
        }

        public static void SelfXorBytes(byte[] bytes, byte[] key, int startIndex, int length)
        {
            if (bytes == null) throw new NullReferenceException("bytes");

            if (key == null) throw new NullReferenceException("key");

            int keyLen = key.Length;
            if (keyLen <= 0) throw new IndexOutOfRangeException("key length");

            int bytesLen = bytes.Length;
            if (startIndex < 0 || length < 0 || startIndex + length > bytesLen)
            {
                throw new IndexOutOfRangeException("index invalid.");
            }

            int keyIndex = startIndex % keyLen;
            for (int i = keyIndex; i < length; i++)
            {
                bytes[i] ^= key[keyIndex++];
                keyIndex %= keyLen;
            }
        }
    }
}