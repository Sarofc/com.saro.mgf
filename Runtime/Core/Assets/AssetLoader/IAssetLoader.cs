using Cysharp.Threading.Tasks;
using System;
using Object = UnityEngine.Object;

namespace Saro.Core
{
    /*
     * TODO  需要易于使用，方便接入其他已有模块，让其可以享受到资源框架的 自动/半自动 卸载功能
     * 
     * 1. 自动回收机制 
     *    - 开启autounload后，handle会经过一定时间后，自动unload
     *    - 如何有效管理 引用？ DefaultAssetLoader/LruAssetLoader目前不能混合使用，看怎么方便处理
     *    
     *    可参考：
     *    https://www.codeleading.com/article/84286014276/
     *    https://www.xasset.pro/docs/memorymgr
     *    
     */

    /// <summary>
    /// 用于处理各个子对象的资源加载，各个Manager可以根据需求实现不同的类
    /// <code>提供一个默认实现 <see cref="DefaultAssetLoader"/></code>
    /// </summary>
    public partial interface IAssetLoader
    {
        /// <summary>
        /// 池化标记
        /// </summary>
        bool Poolable { get; set; }

        /// <summary>
        /// 初始化接口
        /// </summary>
        /// <param name="capacity"></param>
        void Init(int capacity);

        /// <summary>
        /// 加载资源，且缓存handle
        /// </summary>
        Object LoadAssetRef(string assetPath, Type type);

        /// <summary>
        /// 异步加载资源，且缓存handle
        /// </summary>
        UniTask<Object> LoadAssetRefAsync(string assetPath, Type type);

        /// <summary>
        /// 卸载指定资源
        /// </summary>
        /// <param name="assetPath"></param>
        void UnloadAssetRef(string assetPath);

        /// <summary>
        /// 卸载所有资源
        /// </summary>
        void UnloadAllAssetRef();

        /// <summary>
        /// 不计数，直接调用原生接口
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IAssetHandle LoadAsset(string assetPath, Type type);

        /// <summary>
        /// 不计数，直接调用原生接口
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IAssetHandle LoadAssetAsync(string assetPath, Type type);

        #region 泛型加载接口，可选实现

        /// <summary>
        /// 加载资源，且缓存handle
        /// </summary>
        T LoadAssetRef<T>(string assetPath) where T : Object => throw new NotImplementedException("泛型接口未实现");

        /// <summary>
        /// 异步加载资源，且缓存handle
        /// </summary>
        UniTask<T> LoadAssetRefAsync<T>(string assetPath) where T : Object => throw new NotImplementedException("泛型接口未实现");

        /// <summary>
        /// 异步加载资源，且缓存handle
        /// </summary>
        IAssetHandle LoadAssetHandleRefAsync<T>(string assetPath) => throw new NotImplementedException("泛型接口未实现");

        #endregion
    }
}