using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Saro.Utility
{
    public static class HashUtility
    {
        private readonly static MD5 s_Md5 = MD5.Create();
        private readonly static CRC32 s_Crc32 = new();
        private readonly static char[] s_Digitals = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        public static string GetMd5HexHash(string input)
        {
            var data = s_Md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return ToHexString(data);
        }

        public static string GetMd5HexHash(Stream input)
        {
            var data = s_Md5.ComputeHash(input);
            return ToHexString(data);
        }

        public static bool VerifyMd5HexHash(string input, string hash)
        {
            return 0 == StringComparer.OrdinalIgnoreCase.Compare(input, hash);
        }

        public static string GetCrc32HexHash(Stream input)
        {
            var data = s_Crc32.ComputeHash(input);
            return ToHexString(data);
        }

        public static uint GetCrc32(string input)
        {
            if (input.Length <= 512)
            {
                var chrSpan = input.AsSpan();
                //Span<byte> bytesSpan = stackalloc byte[chrSpan.Length * 3]; // 这样更快，但是可能不稳定，且会浪费一定栈空间，如果有比中文更长的话，将会出错
                var bytesLen = Encoding.UTF8.GetByteCount(chrSpan);
                Span<byte> bytesSpan = stackalloc byte[bytesLen];
                Encoding.UTF8.GetBytes(chrSpan, bytesSpan);

                return CRC32.Compute(bytesSpan);
            }
            else
            {
                var bytesSpan = Encoding.UTF8.GetBytes(input);
                return CRC32.Compute(bytesSpan);
            }
        }

        public static uint GetCrc32(byte[] bytes)
        {
            return CRC32.Compute(bytes);
        }

        public static uint GetCrc32(ReadOnlySpan<byte> bytes)
        {
            return CRC32.Compute(bytes);
        }

        public static string GetCrc32HexHash(byte[] bytes)
        {
            var data = s_Crc32.ComputeHash(bytes);
            return ToHexString(data);
        }

        public static string GetCrc32HexHash(string input)
        {
            var data = s_Crc32.ComputeHash(Encoding.UTF8.GetBytes(input));
            return ToHexString(data);
        }

        public static bool VerifyCrc32HexHash(string input, string hash)
        {
            return 0 == StringComparer.OrdinalIgnoreCase.Compare(input, hash);
        }

        public static string ToHexString(ReadOnlySpan<byte> inputBytes)
        {
            const int byteLen = 2;
            var str = new string('\0', byteLen * inputBytes.Length);

            unsafe
            {
                fixed (char* ptr = str)
                {
                    var p = ptr;

                    foreach (var item in inputBytes)
                    {
                        *p++ = s_Digitals[item >> 4]; // byte high
                        *p++ = s_Digitals[item & 15];// byte low
                    }
                }
            }

            return str;
        }
    }

    internal sealed class CRC32 : HashAlgorithm
    {
        private const uint k_DefaultPolynomial = 0xedb88320u;
        private const uint k_DefaultSeed = 0xffffffffu;

        private static uint[] s_DefaultTable;

        private readonly uint m_Seed;
        private readonly uint[] m_Table;
        private uint m_Hash;

        public CRC32()
            : this(k_DefaultPolynomial, k_DefaultSeed)
        { }

        public CRC32(uint polynomial, uint seed)
        {
            if (!BitConverter.IsLittleEndian)
                throw new PlatformNotSupportedException("Not supported on Big Endian processors");

            m_Table = InitializeTable(polynomial);
            m_Seed = m_Hash = seed;
        }

        public override void Initialize()
        {
            m_Hash = m_Seed;
        }

        protected override void HashCore(byte[] buffer, int offset, int count)
        {
            m_Hash = CalculateHash(m_Table, m_Hash, buffer, offset, count);
        }

        protected override void HashCore(ReadOnlySpan<byte> source)
        {
            m_Hash = CalculateHash(m_Table, m_Hash, source, 0, source.Length);
        }

        protected override byte[] HashFinal()
        {
            if (HashValue == null)
                HashValue = new byte[4];

            var hashSpan = HashValue.AsSpan();
            MemoryUtility.GetBytes(~m_Hash, hashSpan);

            return HashValue;
        }

        public override int HashSize => 32;

        public static uint Compute(ReadOnlySpan<byte> buffer)
        {
            return Compute(k_DefaultPolynomial, k_DefaultSeed, buffer, 0, buffer.Length);
        }

        public static uint Compute(ReadOnlySpan<byte> buffer, int offset, int count)
        {
            return Compute(k_DefaultPolynomial, k_DefaultSeed, buffer, offset, count);
        }

        public static uint Compute(uint seed, ReadOnlySpan<byte> buffer)
        {
            return Compute(k_DefaultPolynomial, seed, buffer, 0, buffer.Length);
        }

        public static uint Compute(uint polynomial, uint seed, ReadOnlySpan<byte> buffer, int offset, int count)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, offset, count);
        }

        private static uint[] InitializeTable(uint polynomial)
        {
            if (polynomial == k_DefaultPolynomial && s_DefaultTable != null)
                return s_DefaultTable;

            var createTable = new uint[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (uint)i;
                for (var j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry >>= 1;
                createTable[i] = entry;
            }

            if (polynomial == k_DefaultPolynomial)
                s_DefaultTable = createTable;

            return createTable;
        }

        private static uint CalculateHash(uint[] table, uint seed, ReadOnlySpan<byte> buffer, int ibStart, int ibSize)
        {
            var hash = seed;
            var end = ibStart + ibSize;
            for (var i = ibStart; i < end; i++)
                hash = (hash >> 8) ^ table[buffer[i] ^ hash & 0xff];
            return hash;
        }
    }
}