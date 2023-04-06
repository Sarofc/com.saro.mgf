using Cysharp.Threading.Tasks;
using Saro.Pool;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Saro.Core
{
    public abstract class PoolableGameObject : MonoBehaviour, IHandledObject
    {
        public int ObjectId { get; internal set; }
        /// <summary>
        /// 资源预制体
        /// </summary>
        public PoolableGameObject Prefab { get; internal set; }
        /// <summary>
        /// 从池中获取后调用
        /// </summary>
        public virtual void OnPoolableGet() { }
        /// <summary>
        /// 入池后调用
        /// </summary>
        public virtual void OnPoolableRelease() { }
        /// <summary>
        /// 被销毁时调用
        /// </summary>
        public virtual void OnPoolableDestroy() { }
    }

    public partial class RuntimeGameObjectManager
    {
        private GameObject m_GameObjectPoolRoot;
        private static int s_GlobalObjectID;

        private readonly Dictionary<PoolableGameObject, string> m_Prefab2PathMap = new();
        private readonly Dictionary<string, PoolableGameObject> m_Path2PrefabMap = new();
        private readonly Dictionary<string, IAssetHandle> m_HandleMap = new(StringComparer.Ordinal);
        private readonly Dictionary<PoolableGameObject, GameObjectPool> m_ObjectMap = new();

        private IAssetLoader m_AssetLoader = IAssetLoader.Create<DefaultAssetLoader>(1024, false);

        public PoolableGameObject LoadPrefab(string assetPath)
        {
            if (!m_Path2PrefabMap.TryGetValue(assetPath, out var prefab))
            {
                prefab = m_AssetLoader.LoadAssetRef<PoolableGameObject>(assetPath);

                if (prefab)
                {
                    m_Prefab2PathMap.Add(prefab, assetPath);
                    m_Path2PrefabMap.Add(assetPath, prefab);
                }
            }
            return prefab;
        }

        public async UniTask<PoolableGameObject> LoadPrefabAsync(string assetPath)
        {
            if (!m_Path2PrefabMap.TryGetValue(assetPath, out var prefab))
            {
                prefab = await m_AssetLoader.LoadAssetRefAsync<PoolableGameObject>(assetPath);
                if (prefab)
                {
                    if (!m_Path2PrefabMap.ContainsKey(assetPath))
                    {
                        m_Prefab2PathMap.Add(prefab, assetPath);
                        m_Path2PrefabMap.Add(assetPath, prefab);
                    }
                }
            }
            return prefab;
        }

        public IAssetHandle LoadPrefabHandleAsync(string assetPath)
        {
            if (!m_HandleMap.TryGetValue(assetPath, out var handle))
            {
                handle = m_AssetLoader.LoadAssetHandleRefAsync<PoolableGameObject>(assetPath);
                if (handle.IsDone && !handle.IsError)
                    HandleCompleted(handle);
                else
                    handle.Completed += HandleCompleted;
                m_HandleMap.Add(assetPath, handle);
            }
            return handle;
        }

        private void HandleCompleted(IAssetHandle handle)
        {
            handle.Completed -= HandleCompleted;

            var prefab = handle.Asset as PoolableGameObject;
            if (prefab)
            {
                m_Prefab2PathMap.Add(prefab, handle.AssetUrl);
                m_Path2PrefabMap.Add(handle.AssetUrl, prefab);
            }
        }

        public ObjectHandle<PoolableGameObject> SpawnGameObject(PoolableGameObject prefab)
        {
            if (!m_ObjectMap.TryGetValue(prefab, out var pool))
            {
                pool = CreateGameObjectPool(prefab);
                pool.m_Manager = this;
                pool.m_AssetName = m_Prefab2PathMap[prefab];
                m_ObjectMap.Add(prefab, pool);
            }

            pool.Use();
            var instance = pool.Rent();
            return new(instance);
        }

        public void RecycleGameObject(in ObjectHandle<PoolableGameObject> handle)
        {
            RecycleGameObject(handle.Value);
        }

        public void RecycleGameObject(PoolableGameObject instance)
        {
            if (instance == null)
            {
                Log.ERROR($"[{this.GetType().Name}] {nameof(RecycleGameObject)} failed. instance is null");
                return;
            }

            if (m_ObjectMap.TryGetValue(instance.Prefab, out var pool))
            {
                pool.Return(instance);
            }
            else
            {
                Log.ERROR($"[{this.GetType().Name}] {nameof(RecycleGameObject)} failed. {nameof(instance.Prefab)}: {instance.Prefab} not found");
            }
        }

        public void DestroyAllPooledGameObjects()
        {
            foreach (var item in m_ObjectMap)
            {
                var pool = item.Value;
                pool.Clear();
            }

            m_ObjectMap.Clear();
        }

        public void OnAwake()
        {
            m_GameObjectPoolRoot = new GameObject($"[{this.GetType().Name}]");
            m_GameObjectPoolRoot.SetActive(false);
            GameObject.DontDestroyOnLoad(m_GameObjectPoolRoot);

            m_GameObjectPoolRoot.transform.hierarchyCapacity = 64;
        }

        public void OnUpdate()
        {
            // 自动清理长时间不用的特效池子
            foreach (var item in m_ObjectMap)
            {
                item.Value.AutoUnload();
            }
        }

        public void OnDispose()
        {
            if (m_GameObjectPoolRoot != null)
            {
                GameObject.Destroy(m_GameObjectPoolRoot);
            }
        }
    }

    partial class RuntimeGameObjectManager
    {
        public Config config = new Config
        {
            autoUnload = true,
            unloadTime = 60f * 2,
        };

        public struct Config
        {
            public bool autoUnload;
            public float unloadTime;
        }

        internal class GameObjectPool : ObjectPool<PoolableGameObject>
        {
            public override string Label => m_AssetName;

            internal string m_AssetName;
            internal RuntimeGameObjectManager m_Manager;

            private float m_UsedTime;
            private bool m_Unloaded;

            public GameObjectPool(Func<PoolableGameObject> onCreate, Action<PoolableGameObject> onGet = null, Action<PoolableGameObject> onRelease = null, Action<PoolableGameObject> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) : base(onCreate, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize)
            {
            }

            public GameObjectPool(Func<CancellationToken, UniTask<PoolableGameObject>> onCreateAsync, Action<PoolableGameObject> onGet = null, Action<PoolableGameObject> onRelease = null, Action<PoolableGameObject> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) : base(onCreateAsync, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize)
            {
            }

            internal void Use()
            {
                m_UsedTime = Time.realtimeSinceStartup;
                m_Unloaded = false;
            }

            internal void AutoUnload()
            {
                if (ShouldAutoUnload())
                {
                    Unload();
                }
            }

            private bool ShouldAutoUnload()
            {
                return m_Manager.config.autoUnload
                    && !m_Unloaded
                    && m_UsedTime + m_Manager.config.unloadTime < Time.realtimeSinceStartup;
            }

            private void Unload()
            {
                m_Unloaded = true;
                m_Manager.m_AssetLoader.UnloadAssetRef(m_AssetName);

                Clear();
            }
        }

        void onGet(PoolableGameObject instance)
        {
            instance.transform.SetParent(null);
            instance.ObjectId = ++s_GlobalObjectID;
            instance.OnPoolableGet();
        }
        void onRelease(PoolableGameObject instance)
        {
            instance.gameObject.transform.SetParent(m_GameObjectPoolRoot.transform);
            instance.ObjectId = 0;
            instance.OnPoolableRelease();
        }
        void onDestroy(PoolableGameObject instance)
        {
            instance.OnPoolableDestroy();
            GameObject.Destroy(instance.gameObject);
        }

        private GameObjectPool CreateGameObjectPool(PoolableGameObject prefab)
        {
            return new GameObjectPool(
                    onCreate: () =>
                    {
                        var instance = GameObject.Instantiate(prefab);
                        instance.Prefab = prefab;
                        return instance;
                    },
                    onGet: onGet,
                    onRelease: onRelease,
                    onDestroy: onDestroy
            );
        }
    }

    partial class RuntimeGameObjectManager : IService
    {
        public static RuntimeGameObjectManager Current => Main.Resolve<RuntimeGameObjectManager>();

        void IService.Awake()
        {
            OnAwake();
        }

        void IService.Dispose()
        {
            OnDispose();
        }

        void IService.Update()
        {
            OnUpdate();
        }
    }
}