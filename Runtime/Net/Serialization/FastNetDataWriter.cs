using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Saro.Net
{
    public ref struct FastNetDataWriter
    {
        private IBufferWriter<byte> m_BufferWriter;
        private int m_WrittenCount;

        public int WrittenCount => m_WrittenCount;

        public FastNetDataWriter(IBufferWriter<byte> bufferWriter)
        {
            m_BufferWriter = bufferWriter;
            m_WrittenCount = 0;
        }

        public void WriteUnmanaged<T>(in T obj) where T : unmanaged
        {
            var size = Unsafe.SizeOf<T>();

            ref var p = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref p, obj); // 这里有个结构体拷贝

            Advance(size);
        }

        public void WriteString(string obj)
        {
            var strSpan = obj.AsSpan();

            WriteUnmanaged(strSpan.Length);

            var sizeHint = Encoding.UTF8.GetByteCount(strSpan);
            var span = m_BufferWriter.GetSpan(sizeHint);
            Encoding.UTF8.GetBytes(strSpan, span);
        }

        public void Advance(int count)
        {
            m_BufferWriter.Advance(count);
            m_WrittenCount += count;
        }

        public ref byte GetSpanReference(int sizeHint)
        {
            var span = m_BufferWriter.GetSpan(sizeHint);
            return ref MemoryMarshal.GetReference(span);
        }
    }
}
