using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Saro.Core
{
    /// <summary>
    /// 不可重复资源加载器
    /// <code>精细管理所有资源引用，保证 此加载器 每个资源只被加载一次。持有此加载器的对象生命周期结束时，调用释放接口 <see cref="UnloadAllAssetRef"/> </code>
    /// <code>一般在 `自动卸载AB` 的情况下使用</code>
    /// </summary>
    public sealed class DefaultAssetLoader : IAssetLoader, IReference
    {
        private Dictionary<string, IAssetHandle> m_AssetCache;
        private IAssetManager m_AssetManager => IAssetManager.Current;

        bool IAssetLoader.Poolable { get; set; }

        public DefaultAssetLoader() { }

        void IAssetLoader.Init(int capacity)
        {
            if (m_AssetCache == null)
                m_AssetCache = new Dictionary<string, IAssetHandle>(capacity, StringComparer.Ordinal);
        }

        public T LoadAssetRef<T>(string assetPath) where T : UObject
        {
            return LoadAssetRef(assetPath, typeof(T)) as T;
        }

        public async UniTask<T> LoadAssetRefAsync<T>(string assetPath) where T : UObject
        {
            return await LoadAssetRefAsync(assetPath, typeof(T)) as T;
        }

        public IAssetHandle LoadAssetHandleRefAsync<T>(string assetPath)
        {
            Check(assetPath, typeof(T));

            if (!m_AssetCache.TryGetValue(assetPath, out var handle))
            {
                handle = m_AssetManager.LoadAssetAsync(assetPath, typeof(T));
                m_AssetCache.Add(assetPath, handle);
            }
            return handle;
        }

        public UObject LoadAssetRef(string assetPath, Type type)
        {
            Check(assetPath, type);

            if (!m_AssetCache.TryGetValue(assetPath, out var handle))
            {
                handle = m_AssetManager.LoadAssetAsync(assetPath, type);
                m_AssetCache.Add(assetPath, handle);
            }
            handle.WaitForCompletion();
            return handle.Asset;
        }

        public async UniTask<UObject> LoadAssetRefAsync(string assetPath, Type type)
        {
            Check(assetPath, type);

            if (!m_AssetCache.TryGetValue(assetPath, out var handle))
            {
                handle = m_AssetManager.LoadAssetAsync(assetPath, type);
                m_AssetCache.Add(assetPath, handle);
            }

            if (!handle.IsDone)
                await handle;

#if DEBUG
            if (handle.IsDone == false)
                Log.ERROR($"AssetManager fatal error. url: {assetPath} type: {type}");
#endif

            return handle.Asset;
        }

        [System.Obsolete("use LoadAssetAsync+WaitForCompletion instead", true)]
        public IAssetHandle LoadAsset(string assetPath, Type type)
        {
            return m_AssetManager.LoadAsset(assetPath, type);
        }

        public IAssetHandle LoadAssetAsync(string assetPath, Type type)
        {
            return m_AssetManager.LoadAssetAsync(assetPath, type);
        }

        public void UnloadAssetRef(string assetPath)
        {
            if (m_AssetCache.TryGetValue(assetPath, out var handle))
            {
                handle.DecreaseRefCount();
                m_AssetCache.Remove(assetPath);
            }
        }

        public void UnloadAllAssetRef()
        {
            foreach (var item in m_AssetCache)
            {
                item.Value.DecreaseRefCount();
            }
            m_AssetCache.Clear();
        }

        public void IReferenceClear()
        {
            UnloadAllAssetRef();
        }

        [Conditional("UNITY_EDITOR")]
        private void Check(string assetPath, Type type)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetPath))
            {
                Log.ERROR($"assetPath is null or empty");
            }

            if (typeof(Component).IsAssignableFrom(type))
            {
                Log.ERROR($"not support typeof(Component) {type}, use GameObject instead. url: {Path.GetFileName(assetPath)}");
            }
#endif
        }
    }
}