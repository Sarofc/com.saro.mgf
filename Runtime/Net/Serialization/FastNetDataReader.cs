using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Saro.Net
{
    // TODO 结构体网络数据读取类,直接全部使用大端?
    // 测试下 BitConverter(考虑了大小端) 和 Unmanaged 的效率
    // NetDataReader 对比效率
    // 如果实在要考虑大小端，就用 BinaryPrimitives.ReadInt32LittleEndian
    public ref struct FastNetDataReader
    {
        private ReadOnlySpan<byte> _data;
        private int _position;

        public FastNetDataReader(ReadOnlySpan<byte> buffer, int position = 0)
        {
            this._data = buffer;
            _position = position;
        }

        public void Advance(int size)
        {
            _position += size;
        }

        public T ReadUnmanaged<T>() where T : unmanaged
        {
            var size = Unsafe.SizeOf<short>();
            T result = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(_data.Slice(_position)));
            Advance(size);
            return result;
        }

        public string ReadString()
        {
            // TODO 可以参考 memorypack 来优化 string 读写的效率
            var size = ReadUnmanaged<int>();
            var result = Encoding.UTF8.GetString(GetSpan(size));
            Advance(size);
            return result;
        }

        public ReadOnlySpan<byte> GetSpan(int length)
        {
            return _data.Slice(_position, length);
        }

        public ReadOnlySpan<byte> GetSpan()
        {
            return _data.Slice(_position);
        }
    }
}
