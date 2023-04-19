using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Saro.Utility
{
    internal static class CollectionsMarshalEx
    {
        /// <summary>
        /// similar as AsSpan but modify size to create fixed-size span.
        /// </summary>
        public static Span<T> CreateSpan<T>(List<T> list, int length)
        {
            if (list.Capacity < length)
            {
                list.Capacity = length;
            }

            ref var view = ref Unsafe.As<List<T>, ListView<T>>(ref list);
            view._size = length;
            return view._items.AsSpan(0, length);
        }

        // NOTE: These structure depndent on .NET 7, if changed, require to keep same structure.

        internal sealed class ListView<T>
        {
            public T[] _items;
            public int _size;
            public int _version;
        }
    }
}
