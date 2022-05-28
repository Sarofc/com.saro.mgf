using System;
using System.Collections;

namespace Saro.Core
{
    // TODO 考虑 泛型实现？可以参考下 Addressable 的实现
    public interface IAssetHandle : IRefCount, IEnumerator
    {
        /// <summary>
        /// 请求资源地址
        /// </summary>
        string AssetUrl { get; set; }

        /// <summary>
        /// 请求资源类型
        /// </summary>
        Type AssetType { get; set; }

        /// <summary>
        /// 请求完成回调
        /// </summary>
        Action<IAssetHandle> Completed { get; set; }

        /// <summary>
        /// 加载的对象
        /// </summary>
        UnityEngine.Object Asset { get; }

        /// <summary>
        /// 加载的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetAsset<T>() where T : UnityEngine.Object;

        /// <summary>
        /// 是否完成
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// 加载进度
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// 是否有误
        /// </summary>
        bool IsError { get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        string Error { get; }

        /// <summary>
        /// 加载的字节数组
        /// </summary>
        byte[] Bytes { get; }

        /// <summary>
        /// 加载的字符串
        /// </summary>
        string Text { get; }
    }
}
