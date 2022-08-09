using Cysharp.Threading.Tasks;
using Saro.Collections;
using System;
using Object = UnityEngine.Object;

namespace Saro.Core
{
    /// <summary>
    /// lru资源加载器。这个是xasst给的案例，个人感觉欠考虑
    /// <code>达到上限后，自动卸载最久不用的资源(真正释放资源是在切换scene时)，并加载新资源</code>
    /// <code>只能在 `不自动卸载AB` 的情况下使用，否则会出现已用的资源被卸载掉的情况</code>
    /// </summary>
    public sealed class LruAssetLoader : IAssetLoader
    {
        private TLRUCache<string, IAssetHandle> m_AssetCache;
        private IAssetManager m_AssetManager;

        bool IAssetLoader.Poolable { get; set; }

        public LruAssetLoader() { }

        void IAssetLoader.Init(int capacity)
        {
            m_AssetCache = new TLRUCache<string, IAssetHandle>(capacity, OnLruCacheValueRemoved);
        }

        public void SetAssetInterface(IAssetManager assetManager)
        {
            m_AssetManager = assetManager;
        }

        public bool HasAssetInterface()
        {
            return m_AssetManager != null;
        }

        public T LoadAssetRef<T>(string assetPath) where T : Object
        {
            return LoadAssetRef(assetPath, typeof(T)) as T;
        }

        public async UniTask<T> LoadAssetRefAsync<T>(string assetPath) where T : Object
        {
            return await LoadAssetRefAsync(assetPath, typeof(T)) as T;
        }

        public Object LoadAssetRef(string assetPath, Type type)
        {
            var handle = m_AssetCache.Get(assetPath);

            if (handle == null)
            {
                handle = m_AssetManager.LoadAsset(assetPath, type);
                m_AssetCache.Put(assetPath, handle);
            }

            return handle.Asset;
        }

        public async UniTask<Object> LoadAssetRefAsync(string assetPath, Type type)
        {
            var handle = m_AssetCache.Get(assetPath);

            if (handle == null)
            {
                handle = m_AssetManager.LoadAssetAsync(assetPath, type);
                m_AssetCache.Put(assetPath, handle);
            }

            if (!handle.IsDone)
                await handle;

            return handle.Asset;
        }

        public void UnloadAllAssetRef()
        {
            m_AssetCache.Clear(true);
        }

        public IAssetHandle LoadAsset(string assetPath, Type type)
        {
            return m_AssetManager.LoadAssetAsync(assetPath, type);
        }

        public IAssetHandle LoadAssetAsync(string assetPath, Type type)
        {
            return m_AssetManager.LoadAssetAsync(assetPath, type);
        }

        private void OnLruCacheValueRemoved(IAssetHandle handle)
        {
            handle.DecreaseRefCount();
        }


        #region 基于ID的接口，可选实现

        public T LoadAssetRef<T>(int assetID) where T : Object
        {
            var assetPath = GetAssetPath(assetID);
            return LoadAssetRef<T>(assetPath);
        }

        public UniTask<T> LoadAssetRefAsync<T>(int assetID) where T : Object
        {
            var assetPath = GetAssetPath(assetID);
            return LoadAssetRefAsync<T>(assetPath);
        }

        public Object LoadAssetRef(int assetID, Type type)
        {
            var assetPath = GetAssetPath(assetID);
            return LoadAssetRef(assetPath, type);
        }

        public UniTask<Object> LoadAssetRefAsync(int assetID, Type type)
        {
            var assetPath = GetAssetPath(assetID);
            return LoadAssetRefAsync(assetPath, type);
        }

        public IAssetHandle LoadAsset(int assetID, Type type)
        {
            return m_AssetManager.LoadAsset(assetID, type);
        }

        public IAssetHandle LoadAssetAsync(int assetID, Type type)
        {
            return m_AssetManager.LoadAssetAsync(assetID, type);
        }

        private string GetAssetPath(int assetID)
        {
            var assetTable = m_AssetManager.AssetTable;
            var assetPath = assetTable.GetAssetPath(assetID);
            if (string.IsNullOrEmpty(assetPath))
            {
                Log.ERROR($"assetID({assetID}) is invalid. assetPath is null or empty");
                return null;
            }
            return assetPath;
        }

        #endregion
    }
}