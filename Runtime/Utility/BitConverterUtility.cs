using System;

namespace Saro.Utility
{
    // TODO
    // 1. 搞懂大小端问题，以及规范 encoding.getbytes已经是大端了
    // 2. 完善工具方法

    public static class BitConverterUtility
    {
        public static void ToBigEndianBytes(this byte[] value)
        {
            if (BitConverter.IsLittleEndian)
            {
                var span = value.AsSpan();
                span.Reverse();
            }
        }

        public static void ToLittleEndianBytes(this byte[] value)
        {
            if (!BitConverter.IsLittleEndian)
            {
                var span = value.AsSpan();
                span.Reverse();
            }
        }
    }
}
