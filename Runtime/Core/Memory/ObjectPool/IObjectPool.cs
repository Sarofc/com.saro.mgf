using Cysharp.Threading.Tasks;

namespace Saro.Pool
{
    public interface IObjectPool<T> where T : class
    {
        /// <summary>
        /// 池中数量
        /// </summary>
        int CountInactive { get; }

        /// <summary>
        /// 租借
        /// </summary>
        /// <returns></returns>
        T Rent();

        /// <summary>
        /// 异步租借
        /// </summary>
        /// <returns></returns>
        UniTask<T> RentAsync();

        /// <summary>
        /// 租借
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        PooledObject<T> Rent(out T v);

        /// <summary>
        /// 返还
        /// </summary>
        /// <param name="element"></param>
        void Return(T element);

        /// <summary>
        /// 清理
        /// </summary>
        void Clear();
    }
}
