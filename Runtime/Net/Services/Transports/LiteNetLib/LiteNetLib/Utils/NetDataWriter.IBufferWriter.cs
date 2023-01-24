using System;
using System.Buffers;

namespace LiteNetLib.Utils
{
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
