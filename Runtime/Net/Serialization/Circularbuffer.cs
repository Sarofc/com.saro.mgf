using System;
using System.Collections.Generic;
using System.IO;

namespace Saro.Net
{
    public class CircularBuffer : Stream
    {
        public int ChunkSize = 8192;

        private readonly Queue<byte[]> bufferQueue = new Queue<byte[]>();

        private readonly Queue<byte[]> bufferCache = new Queue<byte[]>();

        public int LastIndex { get; set; }

        public int FirstIndex { get; set; }

        private byte[] lastBuffer;

        public CircularBuffer()
        {
            AddLast();
        }

        public override long Length
        {
            get
            {
                int c = 0;
                if (bufferQueue.Count == 0)
                {
                    c = 0;
                }
                else
                {
                    c = (bufferQueue.Count - 1) * ChunkSize + LastIndex - FirstIndex;
                }
                if (c < 0)
                {
                    Console.WriteLine("CircularBuffer count < 0: {0}, {1}, {2}", bufferQueue.Count, LastIndex, FirstIndex);
                }
                return c;
            }
        }

        public void AddLast()
        {
            byte[] buffer;
            if (bufferCache.Count > 0)
            {
                buffer = bufferCache.Dequeue();
            }
            else
            {
                buffer = new byte[ChunkSize];
            }
            bufferQueue.Enqueue(buffer);
            lastBuffer = buffer;
        }

        public void RemoveFirst()
        {
            bufferCache.Enqueue(bufferQueue.Dequeue());
        }

        public byte[] First
        {
            get
            {
                if (bufferQueue.Count == 0)
                {
                    AddLast();
                }
                return bufferQueue.Peek();
            }
        }

        public byte[] Last
        {
            get
            {
                if (bufferQueue.Count == 0)
                {
                    AddLast();
                }
                return lastBuffer;
            }
        }

        /// <summary>
        /// 从CircularBuffer读到stream中
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        //public async ETTask ReadAsync(Stream stream)
        //{
        //    long buffLength = this.Length;
        //	int sendSize = this.ChunkSize - this.FirstIndex;
        //    if (sendSize > buffLength)
        //    {
        //	    sendSize = (int)buffLength;
        //    }
        //
        //    await stream.WriteAsync(this.First, this.FirstIndex, sendSize);
        //
        //    this.FirstIndex += sendSize;
        //    if (this.FirstIndex == this.ChunkSize)
        //    {
        //	    this.FirstIndex = 0;
        //	    this.RemoveFirst();
        //    }
        //}

        // 从CircularBuffer读到stream
        public void Read(Stream stream, int count)
        {
            if (count > Length)
            {
                throw new Exception($"bufferList length < count, {Length} {count}");
            }

            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                int n = count - alreadyCopyCount;
                if (ChunkSize - FirstIndex > n)
                {
                    stream.Write(First, FirstIndex, n);
                    FirstIndex += n;
                    alreadyCopyCount += n;
                }
                else
                {
                    stream.Write(First, FirstIndex, ChunkSize - FirstIndex);
                    alreadyCopyCount += ChunkSize - FirstIndex;
                    FirstIndex = 0;
                    RemoveFirst();
                }
            }
        }

        // 从stream写入CircularBuffer
        public void Write(Stream stream)
        {
            int count = (int)(stream.Length - stream.Position);

            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                if (LastIndex == ChunkSize)
                {
                    AddLast();
                    LastIndex = 0;
                }

                int n = count - alreadyCopyCount;
                if (ChunkSize - LastIndex > n)
                {
                    stream.Read(lastBuffer, LastIndex, n);
                    LastIndex += count - alreadyCopyCount;
                    alreadyCopyCount += n;
                }
                else
                {
                    stream.Read(lastBuffer, LastIndex, ChunkSize - LastIndex);
                    alreadyCopyCount += ChunkSize - LastIndex;
                    LastIndex = ChunkSize;
                }
            }
        }


        /// <summary>
        ///  从stream写入CircularBuffer
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        //public async ETTask<int> WriteAsync(Stream stream)
        //{
        //    int size = this.ChunkSize - this.LastIndex;
        //
        //    int n = await stream.ReadAsync(this.Last, this.LastIndex, size);
        //
        //    if (n == 0)
        //    {
        //	    return 0;
        //    }
        //
        //    this.LastIndex += n;
        //
        //    if (this.LastIndex == this.ChunkSize)
        //    {
        //	    this.AddLast();
        //	    this.LastIndex = 0;
        //    }
        //
        //    return n;
        //}

        // 把CircularBuffer中数据写入buffer
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer.Length < offset + count)
            {
                throw new Exception($"bufferList length < coutn, buffer length: {buffer.Length} {offset} {count}");
            }

            long length = Length;
            if (length < count)
            {
                count = (int)length;
            }

            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                int n = count - alreadyCopyCount;
                if (ChunkSize - FirstIndex > n)
                {
                    Array.Copy(First, FirstIndex, buffer, alreadyCopyCount + offset, n);
                    FirstIndex += n;
                    alreadyCopyCount += n;
                }
                else
                {
                    Array.Copy(First, FirstIndex, buffer, alreadyCopyCount + offset, ChunkSize - FirstIndex);
                    alreadyCopyCount += ChunkSize - FirstIndex;
                    FirstIndex = 0;
                    RemoveFirst();
                }
            }

            return count;
        }

        // 把buffer写入CircularBuffer中
        public override void Write(byte[] buffer, int offset, int count)
        {
            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                if (LastIndex == ChunkSize)
                {
                    AddLast();
                    LastIndex = 0;
                }

                int n = count - alreadyCopyCount;
                if (ChunkSize - LastIndex > n)
                {
                    Array.Copy(buffer, alreadyCopyCount + offset, lastBuffer, LastIndex, n);
                    LastIndex += count - alreadyCopyCount;
                    alreadyCopyCount += n;
                }
                else
                {
                    Array.Copy(buffer, alreadyCopyCount + offset, lastBuffer, LastIndex, ChunkSize - LastIndex);
                    alreadyCopyCount += ChunkSize - LastIndex;
                    LastIndex = ChunkSize;
                }
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Position { get; set; }
    }
}