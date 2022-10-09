using System.Collections.Generic;

namespace Saro.Pool
{
    public class StackPool<TItem>
    {
        public static Stack<TItem> Rent()
        {
            return StackPool<TItem>.s_Pool.Rent();
        }

        public static PooledObject<Stack<TItem>> Rent(out Stack<TItem> value)
        {
            return StackPool<TItem>.s_Pool.Rent(out value);
        }

        public static void Return(Stack<TItem> toRelease)
        {
            StackPool<TItem>.s_Pool.Return(toRelease);
        }

        internal static readonly ObjectPool<Stack<TItem>> s_Pool = new ObjectPool<Stack<TItem>>(() => new Stack<TItem>(), null, (l) => l.Clear(), null, true, 10, 10000);
    }
}
