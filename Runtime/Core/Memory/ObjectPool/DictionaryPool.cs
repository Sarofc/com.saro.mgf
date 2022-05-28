using System.Collections.Generic;

namespace Saro.Pool
{
    /// <summary>
    ///   <para>A version of Pool.CollectionPool_2 for Dictionaries.</para>
    /// </summary>
    public class DictionaryPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>
    {
    }
}
