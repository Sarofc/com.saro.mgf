using System;
using System.Buffers;
using System.Net;
using System.Text;

namespace LiteNetLib.Utils
{
    public partial class NetDataWriter
    {
        protected byte[] _data;
        protected int _position;
        private const int InitialSize = 64;
        private readonly bool _autoResize;

        public int Capacity
        {
            get { return _data.Length; }
        }

        public NetDataWriter() : this(true, InitialSize)
        {
        }

        public NetDataWriter(bool autoResize) : this(autoResize, InitialSize)
        {
        }

        public NetDataWriter(bool autoResize, int initialSize)
        {
            _data = new byte[initialSize];
            _autoResize = autoResize;
        }

        /// <summary>
        /// Creates NetDataWriter from existing ByteArray
        /// </summary>
        /// <param name="bytes">Source byte array</param>
        /// <param name="copy">Copy array to new location or use existing</param>
        public static NetDataWriter FromBytes(byte[] bytes, bool copy)
        {
            if (copy)
            {
                var netDataWriter = new NetDataWriter(true, bytes.Length);
                netDataWriter.Put(bytes);
                return netDataWriter;
            }
            return new NetDataWriter(true, 0) { _data = bytes, _position = bytes.Length };
        }

        /// <summary>
        /// Creates NetDataWriter from existing ByteArray (always copied data)
        /// </summary>
        /// <param name="bytes">Source byte array</param>
        /// <param name="offset">Offset of array</param>
        /// <param name="length">Length of array</param>
        public static NetDataWriter FromBytes(byte[] bytes, int offset, int length)
        {
            var netDataWriter = new NetDataWriter(true, bytes.Length);
            netDataWriter.Put(bytes, offset, length);
            return netDataWriter;
        }

        public static NetDataWriter FromString(string value)
        {
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put(value);
            return netDataWriter;
        }

        public void ResizeIfNeed(int newSize)
        {
            int len = _data.Length;
            if (len < newSize)
            {
                while (len < newSize)
                    len *= 2;
                Array.Resize(ref _data, len);
            }
        }

        public void Reset(int size)
        {
            ResizeIfNeed(size);
            _position = 0;
        }

        public void Reset()
        {
            _position = 0;
        }

        public byte[] CopyData()
        {
            byte[] resultData = new byte[_position];
            Buffer.BlockCopy(_data, 0, resultData, 0, _position);
            return resultData;
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public int Length
        {
            get { return _position; }
        }

        /// <summary>
        /// Sets position of NetDataWriter to rewrite previous values
        /// </summary>
        /// <param name="position">new byte position</param>
        /// <returns>previous position of data writer</returns>
        public int SetPosition(int position)
        {
            int prevPosition = _position;
            _position = position;
            return prevPosition;
        }

        public void Put(float value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 4);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 4;
        }

        public void Put(double value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 8);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 8;
        }

        public void Put(long value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 8);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 8;
        }

        public void Put(ulong value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 8);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 8;
        }

        public void Put(int value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 4);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 4;
        }

        public void Put(uint value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 4);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 4;
        }

        public void Put(char value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 2);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 2;
        }

        public void Put(ushort value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 2);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 2;
        }

        public void Put(short value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 2);
            FastBitConverter.GetBytes(_data, _position, value);
            _position += 2;
        }

        public void Put(sbyte value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 1);
            _data[_position] = (byte)value;
            _position++;
        }

        public void Put(byte value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 1);
            _data[_position] = value;
            _position++;
        }

        public void Put(byte[] data, int offset, int length)
        {
            if (_autoResize)
                ResizeIfNeed(_position + length);
            Buffer.BlockCopy(data, offset, _data, _position, length);
            _position += length;
        }

        public void Put(byte[] data)
        {
            if (_autoResize)
                ResizeIfNeed(_position + data.Length);
            Buffer.BlockCopy(data, 0, _data, _position, data.Length);
            _position += data.Length;
        }

        public void PutSBytesWithLength(sbyte[] data, int offset, int length)
        {
            if (_autoResize)
                ResizeIfNeed(_position + length + 4);
            FastBitConverter.GetBytes(_data, _position, length);
            Buffer.BlockCopy(data, offset, _data, _position + 4, length);
            _position += length + 4;
        }

        public void PutSBytesWithLength(sbyte[] data)
        {
            if (_autoResize)
                ResizeIfNeed(_position + data.Length + 4);
            FastBitConverter.GetBytes(_data, _position, data.Length);
            Buffer.BlockCopy(data, 0, _data, _position + 4, data.Length);
            _position += data.Length + 4;
        }

        public void PutBytesWithLength(byte[] data, int offset, int length)
        {
            if (_autoResize)
                ResizeIfNeed(_position + length + 4);
            FastBitConverter.GetBytes(_data, _position, length);
            Buffer.BlockCopy(data, offset, _data, _position + 4, length);
            _position += length + 4;
        }

        public void PutBytesWithLength(byte[] data)
        {
            if (_autoResize)
                ResizeIfNeed(_position + data.Length + 4);
            FastBitConverter.GetBytes(_data, _position, data.Length);
            Buffer.BlockCopy(data, 0, _data, _position + 4, data.Length);
            _position += data.Length + 4;
        }

        public void Put(bool value)
        {
            if (_autoResize)
                ResizeIfNeed(_position + 1);
            _data[_position] = (byte)(value ? 1 : 0);
            _position++;
        }

        private void PutArray(Array arr, int sz)
        {
            ushort length = arr == null ? (ushort)0 : (ushort)arr.Length;
            sz *= length;
            if (_autoResize)
                ResizeIfNeed(_position + sz + 2);
            FastBitConverter.GetBytes(_data, _position, length);
            if (arr != null)
                Buffer.BlockCopy(arr, 0, _data, _position + 2, sz);
            _position += sz + 2;
        }

        public void PutArray(float[] value)
        {
            PutArray(value, 4);
        }

        public void PutArray(double[] value)
        {
            PutArray(value, 8);
        }

        public void PutArray(long[] value)
        {
            PutArray(value, 8);
        }

        public void PutArray(ulong[] value)
        {
            PutArray(value, 8);
        }

        public void PutArray(int[] value)
        {
            PutArray(value, 4);
        }

        public void PutArray(uint[] value)
        {
            PutArray(value, 4);
        }

        public void PutArray(ushort[] value)
        {
            PutArray(value, 2);
        }

        public void PutArray(short[] value)
        {
            PutArray(value, 2);
        }

        public void PutArray(bool[] value)
        {
            PutArray(value, 1);
        }

        public void PutArray(string[] value)
        {
            ushort len = value == null ? (ushort)0 : (ushort)value.Length;
            Put(len);
            for (int i = 0; i < len; i++)
                Put(value[i]);
        }

        public void PutArray(string[] value, int maxLength)
        {
            ushort len = value == null ? (ushort)0 : (ushort)value.Length;
            Put(len);
            for (int i = 0; i < len; i++)
                Put(value[i], maxLength);
        }

        public void Put(IPEndPoint endPoint)
        {
            Put(endPoint.Address.ToString());
            Put(endPoint.Port);
        }

        public void Put(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Put(0);
                return;
            }

            //put bytes count
            int bytesCount = Encoding.UTF8.GetByteCount(value);
            if (_autoResize)
                ResizeIfNeed(_position + bytesCount + 4);
            Put(bytesCount);

            //put string
            Encoding.UTF8.GetBytes(value, 0, value.Length, _data, _position);
            _position += bytesCount;
        }

        public void Put(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                Put(0);
                return;
            }

            int length = value.Length > maxLength ? maxLength : value.Length;
            //calculate max count
            int bytesCount = Encoding.UTF8.GetByteCount(value);
            if (_autoResize)
                ResizeIfNeed(_position + bytesCount + 4);

            //put bytes count
            Put(bytesCount);

            //put string
            Encoding.UTF8.GetBytes(value, 0, length, _data, _position);

            _position += bytesCount;
        }

        public void Put<T>(T obj) where T : INetSerializable
        {
            obj.Serialize(this);
        }
    }

    partial class NetDataWriter : IBufferWriter<byte>
    {
        private const int ArrayMaxLength = 0x7FFFFFC7;

        private const int DefaultInitialBufferSize = 256;

        public int FreeCapacity => _data.Length - _position;

        private void CheckAndResizeBuffer(int sizeHint)
        {
            if (sizeHint < 0)
                throw new ArgumentException(nameof(sizeHint));

            if (sizeHint == 0)
            {
                sizeHint = 1;
            }

            if (sizeHint > FreeCapacity)
            {
                int currentLength = _data.Length;

                // Attempt to grow by the larger of the sizeHint and double the current size.
                int growBy = Math.Max(sizeHint, currentLength);

                if (currentLength == 0)
                {
                    growBy = Math.Max(growBy, DefaultInitialBufferSize);
                }

                int newSize = currentLength + growBy;

                if ((uint)newSize > int.MaxValue)
                {
                    // Attempt to grow to ArrayMaxLength.
                    uint needed = (uint)(currentLength - FreeCapacity + sizeHint);
                    //Debug.Assert(needed > currentLength);

                    if (needed > ArrayMaxLength)
                    {
                        //ThrowOutOfMemoryException(needed);
                        throw new OutOfMemoryException($"Cannot allocate a buffer of size {needed}.");
                    }

                    newSize = ArrayMaxLength;
                }

                Array.Resize(ref _data, newSize);
            }

            //Debug.Assert(FreeCapacity > 0 && FreeCapacity >= sizeHint);
        }

        public void Advance(int count)
        {
            if (count < 0)
                throw new ArgumentException(null, nameof(count));

            if (_position > _data.Length - count)
                throw new InvalidOperationException($"Cannot advance past the end of the buffer, which has a size of {_data.Length}.");

            _position += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            //Debug.Assert(_buffer.Length > _index);
            return _data.AsMemory(_position);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            //Debug.Assert(_buffer.Length > _index);
            return _data.AsSpan(_position);
        }
    }
}
