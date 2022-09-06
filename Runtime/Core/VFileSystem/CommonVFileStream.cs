using System.IO;

namespace Saro.IO
{
    /// <summary>
    /// FileStream wrapper
    /// </summary>
    public sealed class CommonVFileStream : VFileStream
    {
        private readonly FileStream m_FileStream;

        public CommonVFileStream(string fullPath, FileMode mode, FileAccess access)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new System.Exception("fullPath is invalid.");
            }

            m_FileStream = new FileStream(fullPath, mode, access, FileShare.Read); // TODO 文件占用问题测试
        }

        protected internal override long Position
        {
            get => m_FileStream.Position;
            set => m_FileStream.Position = value;
        }

        protected internal override long Length => m_FileStream.Length;

        protected internal override void Close() => m_FileStream.Close();

        protected internal override void Flush() => m_FileStream.Flush();

        protected internal override int Read(byte[] array, int offset, int count)
            => m_FileStream.Read(array, offset, count);

        protected internal override int ReadByte() => m_FileStream.ReadByte();

        protected internal override void Seek(long offset, SeekOrigin origin)
            => m_FileStream.Seek(offset, origin);

        protected internal override void SetLength(long value)
            => m_FileStream.SetLength(value);

        protected internal override void Write(byte[] array, int offset, int count)
            => m_FileStream.Write(array, offset, count);

        protected internal override void WriteByte(byte value)
            => m_FileStream.WriteByte(value);

        public override void Dispose()
        {
            m_FileStream.Dispose();

#if false
#if UNITY_EDITOR
            UnityEngine.Debug.LogError("CommonVFileSystemStream::Dispose");
#else
            Console.WriteLine("CommonVFileSystemStream::Dispose");
#endif
#endif
        }
    }
}