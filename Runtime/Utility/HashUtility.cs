#if UNITY_2021_3_OR_NEWER
using SkipLocalsInitAttribute = Unity.Burst.CompilerServices.SkipLocalsInitAttribute;
#else
using SkipLocalsInitAttribute = System.Runtime.CompilerServices.SkipLocalsInitAttribute;
#endif

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Buffers;
using Saro.Core;

namespace Saro.Utility
{
    public static class HashUtility
    {
        private readonly static MD5 s_Md5 = MD5.Create();
        private readonly static CRC32 s_Crc32 = new();
        private readonly static char[] s_Digitals = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        public static string GetMd5HexHash(ReadOnlySpan<char> chars)
        {
            var byteCount = Encoding.UTF8.GetByteCount(chars);
            if (byteCount <= 512)
            {
                Span<byte> input = stackalloc byte[byteCount];
                Encoding.UTF8.GetBytes(chars, input);
                Span<byte> output = stackalloc byte[16]; // 128 / 8
                bool result = s_Md5.TryComputeHash(input, output, out int bytesWritten);
                Log.Assert(result == true, $"md5 failed. bytesWritten: {bytesWritten}");
                return ToHexString(output);
            }
            else
            {
                var input = ArrayPool<byte>.Shared.Rent(byteCount);
                Encoding.UTF8.GetBytes(chars, input);
                Span<byte> output = stackalloc byte[16];
                bool result = s_Md5.TryComputeHash(input, output, out int bytesWritten);
                Log.Assert(result == true, $"md5 failed. bytesWritten: {bytesWritten}");
                var hashString = ToHexString(output);
                ArrayPool<byte>.Shared.Return(input);
                return hashString;
            }
        }

        internal static string ___GetMd5HexHash(string chars)
        {
            var data = s_Md5.ComputeHash(Encoding.UTF8.GetBytes(chars));
            return ToHexString(data);
        }

        public static string GetMd5HexHash(Stream input)
        {
            // gc alloc ?
            var data = s_Md5.ComputeHash(input); // .net standard 2.1 没有 stream span的重载
            return ToHexString(data);
        }

        public static bool VerifyMd5HexHash(ReadOnlySpan<char> input, ReadOnlySpan<char> hash)
        {
            return input.SequenceEqual(hash);
            //return 0 == StringComparer.OrdinalIgnoreCase.Compare(input, hash);
        }

        public static string GetCrc32HexHash(Stream input)
        {
            var data = s_Crc32.ComputeHash(input);
            return ToHexString(data);
        }

        [SkipLocalsInit]
        public static uint GetCrc32(ReadOnlySpan<char> chars)
        {
            var byteCount = Encoding.UTF8.GetByteCount(chars);
            if (byteCount <= 512)
            {
                //Span<byte> bytesSpan = stackalloc byte[chrSpan.Length * 3]; // 这样更快，但是可能不稳定，且会浪费一定栈空间，如果有比中文更长的话，将会出错。memorypack 是这样处理的，估计是可行的

                Span<byte> bytes = stackalloc byte[byteCount];
                Encoding.UTF8.GetBytes(chars, bytes);
                return CRC32.Compute(bytes);
            }
            else
            {
                var bytes = ArrayPool<byte>.Shared.Rent(byteCount);
                Encoding.UTF8.GetBytes(chars, bytes);
                var hash = CRC32.Compute(bytes);
                ArrayPool<byte>.Shared.Return(bytes); // 应该不用清理，但其他地方需要保证使用长度不出错
                return hash;
            }
        }

        public static uint GetCrc32(ReadOnlySpan<byte> bytes)
        {
            return CRC32.Compute(bytes);
        }

        public static string GetCrc32HexHash(ReadOnlySpan<byte> bytes)
        {
            Span<byte> dst = stackalloc byte[CRC32.k_HashInBytes];
            var result = s_Crc32.TryComputeHash(bytes, dst, out var bytesWritten);
            Log.Assert(result == true, "GetCrc32HexHash failed");
            return ToHexString(dst);
        }

        [SkipLocalsInit]
        public static string GetCrc32HexHash(ReadOnlySpan<char> chars)
        {
            var byteCount = Encoding.UTF8.GetByteCount(chars);
            if (byteCount <= 512)
            {
                //Span<byte> bytesSpan = stackalloc byte[chrSpan.Length * 3]; // 这样更快，但是可能不稳定，且会浪费一定栈空间，如果有比中文更长的话，将会出错。memorypack 是这样处理的，估计是可行的

                Span<byte> src = stackalloc byte[byteCount];
                Encoding.UTF8.GetBytes(chars, src);
                Span<byte> dst = stackalloc byte[CRC32.k_HashInBytes];
                var result = s_Crc32.TryComputeHash(src, dst, out var bytesWritten);
                Log.Assert(result == true, "GetCrc32HexHash failed. stackalloc");
                return ToHexString(dst);
            }
            else
            {
                var src = ArrayPool<byte>.Shared.Rent(byteCount);
                Encoding.UTF8.GetBytes(chars, src);
                Span<byte> dst = stackalloc byte[CRC32.k_HashInBytes];
                var result = s_Crc32.TryComputeHash(src, dst, out var bytesWritten);
                Log.Assert(result == true, "GetCrc32HexHash failed. arraypool");
                var hashString = ToHexString(dst);
                ArrayPool<byte>.Shared.Return(src); // 应该不用清理，但其他地方需要保证使用长度不出错
                return hashString;
            }
        }

        public static bool VerifyCrc32HexHash(ReadOnlySpan<char> input, ReadOnlySpan<char> hash)
        {
            return input.SequenceEqual(hash);
            //return 0 == StringComparer.OrdinalIgnoreCase.Compare(input, hash);
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

        public const int k_HashInBytes = 4;

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

            HashValue = new byte[k_HashInBytes];

            m_Table = InitializeTable(polynomial);
            m_Seed = m_Hash = seed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Initialize()
        {
            //m_Hash = m_Seed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void HashCore(byte[] buffer, int offset, int count)
        {
            m_Hash = CalculateHash(m_Table, m_Seed, buffer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void HashCore(ReadOnlySpan<byte> source)
        {
            m_Hash = CalculateHash(m_Table, m_Seed, source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override byte[] HashFinal()
        {
            ref var pHashValue = ref MemoryMarshal.GetReference<byte>(HashValue);
            Unsafe.WriteUnaligned(ref pHashValue, ~m_Hash);

            return HashValue;
        }

        protected override bool TryHashFinal(Span<byte> destination, out int bytesWritten)
        {
            bytesWritten = k_HashInBytes;
            if (destination.Length < k_HashInBytes) return false;

            //ref var pSrc = ref MemoryMarshal.GetReference<byte>(HashValue);
            //Unsafe.WriteUnaligned(ref pSrc, ~m_Hash);

            var hash = ~m_Hash;
            ref var pSrc = ref Unsafe.As<uint, byte>(ref hash);

            ref var pDst = ref MemoryMarshal.GetReference<byte>(destination);
            Unsafe.CopyBlockUnaligned(ref pDst, ref pSrc, (uint)bytesWritten);

            return true;
        }

        public override int HashSize => 32;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(ReadOnlySpan<byte> bytes)
        {
            return Compute(k_DefaultPolynomial, k_DefaultSeed, bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint seed, ReadOnlySpan<byte> bytes)
        {
            return Compute(k_DefaultPolynomial, seed, bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Compute(uint polynomial, uint seed, ReadOnlySpan<byte> bytes)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, bytes);
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

        private static uint CalculateHash(ReadOnlySpan<uint> table, uint seed, ReadOnlySpan<byte> bytes)
        {
            var hash = seed;
            var end = bytes.Length;
            for (var i = 0; i < end; i++)
                hash = (hash >> 8) ^ table[(int)(bytes[i] ^ hash & 0xff)];
            return hash;
        }

        private static uint CalculateHash(uint[] table, uint seed, byte[] bytes, int start, int size)
        {
            var span = bytes.AsSpan().Slice(start, size);
            return CalculateHash(table, seed, span);

            //var hash = seed;
            //var end = start + size;

            //for (var i = ibStart; i < end; i++)
            //    hash = (hash >> 8) ^ table[bytes[i] ^ hash & 0xff];
            //return hash;
        }
    }
}