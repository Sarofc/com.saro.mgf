using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Saro.Core
{
    /// <summary>
    /// 不可重复资源加载器
    /// <code>精细管理所有资源引用，保证 此加载器 每个资源只被加载一次。持有此加载器的对象生命周期结束时，调用释放接口 <see cref="UnloadAllAssetRef"/> </code>
    /// <code>一般在 `自动卸载AB` 的情况下使用</code>
    /// </summary>
    public sealed class DefaultAssetLoader : IAssetLoader, IReference
    {
        private IAssetManager m_AssetManager;

        private Dictionary<string, IAssetHandle> m_AssetCache;

        public bool Poolable { get; private set; }

        /// <summary>
        /// ERROR: 无参构造，给IReference使用，其他人不要调用
        /// </summary>
        public DefaultAssetLoader()
        {
        }

        public static DefaultAssetLoader Create(int capacity, bool poolable)
        {
            DefaultAssetLoader loader;
            if (poolable)
            {
                loader = SharedPool.Rent<DefaultAssetLoader>();
            }
            else
            {
                loader = new DefaultAssetLoader();
            }

            loader.Init(capacity);
            loader.Poolable = poolable;
            return loader;
        }


        public static void Release(IAssetLoader assetLoader)
        {
            if (assetLoader is DefaultAssetLoader _loader)
            {
                if (_loader.Poolable)
                {
                    SharedPool.Return(_loader);
                }
            }
        }

        private void Init(int capacity)
        {
            if (m_AssetManager == null)
                m_AssetManager = Main.Resolve<IAssetManager>();

            if (m_AssetCache == null)
                m_AssetCache = new Dictionary<string, IAssetHandle>(capacity);
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
            if (string.IsNullOrEmpty(assetPath))
            {
                Log.ERROR($"assetPath is null or empty");
                return null;
            }

            if (!m_AssetCache.TryGetValue(assetPath, out var handle))
            {
                handle = m_AssetManager.LoadAsset(assetPath, type);
                m_AssetCache.Add(assetPath, handle);
            }

            return handle.Asset;
        }

        public async UniTask<Object> LoadAssetRefAsync(string assetPath, Type type)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                Log.ERROR($"assetPath is null or empty");
                return null;
            }

            if (!m_AssetCache.TryGetValue(assetPath, out var handle))
            {
                handle = m_AssetManager.LoadAssetAsync(assetPath, type);
                m_AssetCache.Add(assetPath, handle);
            }

            if (!handle.IsDone)
                await handle;

            return handle.Asset;
        }

        public IAssetHandle LoadAsset(string assetPath, Type type)
        {
            return m_AssetManager.LoadAsset(assetPath, type);
        }

        public IAssetHandle LoadAssetAsync(string assetPath, Type type)
        {
            return m_AssetManager.LoadAssetAsync(assetPath, type);
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