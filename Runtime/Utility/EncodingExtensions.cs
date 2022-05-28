using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Saro.Utility
{
    /*
     * TODO 暂时 从.net 6抄过来, 后面看看有没有nuget直接支持
     * 
     */
    public static class EncodingExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetFirstSpan<T>(this ReadOnlySequence<T> self, out ReadOnlySpan<T> first, out SequencePosition next)
        {
            first = new ReadOnlySpan<T>();
            next = new SequencePosition();
            object startObject = self.Start.GetObject();
            int startInteger = self.Start.GetInteger();
            if (startObject == null)
                return;
            bool hasMultipleSegments = startObject != self.End.GetObject();
            int endInteger = self.End.GetInteger();
            if (startInteger >= 0)
            {
                if (endInteger >= 0)
                {
                    ReadOnlySequenceSegment<T> onlySequenceSegment = (ReadOnlySequenceSegment<T>) startObject;
                    first = onlySequenceSegment.Memory.Span;
                    if (hasMultipleSegments)
                    {
                        first = first.Slice(startInteger);
                        next = new SequencePosition((object) onlySequenceSegment.Next, 0);
                    }
                    else
                        first = first.Slice(startInteger, endInteger - startInteger);
                }
                else
                {
                    if (hasMultipleSegments)
                    {
                        throw new Exception("see ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();");
                    }

                    first = new ReadOnlySpan<T>((T[]) startObject, startInteger, (endInteger & int.MaxValue) - startInteger);
                }
            }
            else
                first = GetFirstSpanSlow<T>(startObject, startInteger, endInteger, hasMultipleSegments);
            // first = ReadOnlySequence<T>.GetFirstSpanSlow(startObject, startInteger, endInteger, hasMultipleSegments);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ReadOnlySpan<T> GetFirstSpanSlow<T>(
            object startObject,
            int startIndex,
            int endIndex,
            bool hasMultipleSegments)
        {
            if (hasMultipleSegments)
                throw new Exception("see ThrowHelper.ThrowInvalidOperationException_EndPositionNotReached();");
            if (typeof(T) == typeof(char) && endIndex < 0)
            {
                ReadOnlySpan<char> span = ((string) startObject).AsSpan(startIndex & int.MaxValue, endIndex - startIndex);
                return MemoryMarshal.CreateReadOnlySpan<T>(ref Unsafe.As<char, T>(ref MemoryMarshal.GetReference<char>(span)), span.Length);
            }

            startIndex &= int.MaxValue;
            return (ReadOnlySpan<T>) ((MemoryManager<T>) startObject).Memory.Span.Slice(startIndex, endIndex - startIndex);
        }

        public static string GetString(this Encoding encoding, in ReadOnlySequence<byte> bytes)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (bytes.IsSingleSegment)
                return encoding.GetString(bytes.FirstSpan);
            Decoder decoder = encoding.GetDecoder();
            List<(char[], int)> state = new List<(char[], int)>();
            int length = 0;
            ReadOnlySequence<byte> readOnlySequence = bytes;
            bool isSingleSegment;
            do
            {
                ReadOnlySpan<byte> first;
                SequencePosition next;
                readOnlySequence.GetFirstSpan(out first, out next);
                isSingleSegment = readOnlySequence.IsSingleSegment;
                char[] chars1 = ArrayPool<char>.Shared.Rent(decoder.GetCharCount(first, isSingleSegment));
                int chars2 = decoder.GetChars(first, (Span<char>) chars1, isSingleSegment);
                state.Add((chars1, chars2));
                length += chars2;
                if (length < 0)
                {
                    length = int.MaxValue;
                    break;
                }

                readOnlySequence = readOnlySequence.Slice(next);
            } while (!isSingleSegment);

            return string.Create<List<(char[], int)>>(length, state, (SpanAction<char, List<(char[], int)>>) ((span, listOfSegments) =>
            {
                foreach ((char[] array, int num) in listOfSegments)
                {
                    array.AsSpan<char>(0, num).CopyTo(span);
                    ArrayPool<char>.Shared.Return(array);
                    span = span.Slice(num);
                }
            }));
        }
    }
}