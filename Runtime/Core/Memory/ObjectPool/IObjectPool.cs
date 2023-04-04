using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;


namespace Saro.Pool
{
    public interface IObjectPool
    {
        /// <summary>
        /// 对象池标签
        /// </summary>
        string Label { get; }
        /// <summary>
        /// 所有数量
        /// </summary>
        int CountAll { get; }
        /// <summary>
        /// 池中数量
        /// </summary>
        int CountInactive { get; }
        /// <summary>
        /// 使用数量
        /// </summary>
        int CountActive { get; }
        int RentCount { get; }
        int ReturnCount { get; }
    }

    public interface IObjectPool<T> : IObjectPool where T : class
    {
        /// <summary>
        /// 租借
        /// </summary>
        /// <returns></returns>
        T Rent();

        /// <summary>
        /// 异步租借
        /// </summary>
        /// <returns></returns>
        UniTask<T> RentAsync(CancellationToken cancellationToken = default);

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
