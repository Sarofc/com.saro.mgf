using System;
using System.IO;

namespace Saro.IO
{
    public abstract class VFileStream : IDisposable
    {
        protected const int k_CachedBytesLength = 0x1000;
        protected static readonly byte[] s_CachedBytes = new byte[k_CachedBytesLength];

        protected internal abstract long Position { get; set; }

        protected internal abstract long Length { get; }

        protected internal abstract void SetLength(long length);

        protected internal abstract void Seek(long offset, SeekOrigin origin);

        protected internal abstract int ReadByte();

        protected internal abstract int Read(byte[] array, int offset, int count);

        protected internal int Read(Stream stream, int length)
        {
            int bytesLeft = length;

            int bytesRead;
            while ((bytesRead = Read(s_CachedBytes, 0, bytesLeft < k_CachedBytesLength ? bytesLeft : k_CachedBytesLength)) > 0)
            {
                bytesLeft -= bytesRead;
                stream.Write(s_CachedBytes, 0, bytesRead);
            }

            Array.Clear(s_CachedBytes, 0, k_CachedBytesLength);
            return length - bytesLeft;
        }

        protected internal abstract void WriteByte(byte value);

        protected internal abstract void Write(byte[] array, int offset, int count);

        // TODO span api
        //protected internal abstract void Write(ReadOnlySpan<byte> span);

        protected internal void Write(Stream stream, int count)
        {
            int bytesLeft = count;

            int bytesRead;
            while ((bytesRead = stream.Read(s_CachedBytes, 0, bytesLeft < k_CachedBytesLength ? bytesLeft : k_CachedBytesLength)) > 0)
            {
                bytesLeft -= bytesRead;
                Write(s_CachedBytes, 0, bytesRead);
            }

            Array.Clear(s_CachedBytes, 0, k_CachedBytesLength);
        }

        protected internal abstract void Flush();

        protected internal abstract void Close();

        public abstract void Dispose();

        // TODO async support
        //protected internal abstract int ReadAsync(byte[] array, int offset, int count);
        //protected internal UniTask<int> ReadAsync(Stream stream, int length)
        //{
        //    throw new NotImplementedException(); // TODO
        //}
    }
}
