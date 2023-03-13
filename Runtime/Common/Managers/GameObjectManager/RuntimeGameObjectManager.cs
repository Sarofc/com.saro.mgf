using Cysharp.Threading.Tasks;
using Saro.Pool;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saro.Core
{
    public abstract class PoolableGameObject : MonoBehaviour, IHandledObject
    {
        public int ObjectID { get; internal set; }
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; internal set; }
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

    public partial class RuntimeGameObjectManager<T> where T : PoolableGameObject
    {
        public Config config = new()
        {
            autoUnload = true,
            unloadTime = 60f,
        };

        private GameObject m_GameObjectPoolRoot;
        private static int s_GlobalObjectID;

        public string PrefixPath { get; private set; }

        private readonly Dictionary<string, GameObjectPool> m_ObjectMap = new(StringComparer.Ordinal);

        private readonly IAssetLoader m_AssetLoader = AssetLoaderFactory.Create<DefaultAssetLoader>(128);

        public void SetPrefixPath(string prefixPath)
        {
            PrefixPath = prefixPath;
        }

        public async UniTask<ObjectHandle<T>> SpawnGameObjectAsync(string assetPath)
        {
            if (!m_ObjectMap.TryGetValue(assetPath, out var pool))
                pool = CreateGameObjectPool(assetPath, true);

            pool.Use();
            var instance = await pool.RentAsync();
            return new(instance);
        }

        public ObjectHandle<T> SpawnGameObject(string assetPath)
        {
            if (!m_ObjectMap.TryGetValue(assetPath, out var pool))
                pool = CreateGameObjectPool(assetPath, true);

            pool.Use();
            var instance = pool.Rent();
            return new(instance);
        }

        public void RecycleGameObject(in ObjectHandle<T> handle)
        {
            if (handle)
            {
                RecycleGameObject(handle.Object);
            }
        }

        public void RecycleGameObject(T instance)
        {
            if (instance == null)
            {
                Log.ERROR($"[{this.GetType().Name}] {nameof(RecycleGameObject)} failed. Effect is null");
                return;
            }

            if (m_ObjectMap.TryGetValue(instance.AssetPath, out var pool))
            {
                pool.Return(instance);
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

    partial class RuntimeGameObjectManager<T>
    {
        public struct Config
        {
            public bool autoUnload;
            public float unloadTime;
        }

        private class GameObjectPool : ObjectPool<T>
        {
            private string m_AssetName;
            private float m_UsedTime;
            private RuntimeGameObjectManager<T> m_Manager;
            private bool m_Unloaded;

            public GameObjectPool(RuntimeGameObjectManager<T> manager, string assetName, Func<T> onCreate, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) : base(onCreate, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize)
            {
                m_Manager = manager;
                m_AssetName = assetName;
            }

            public GameObjectPool(RuntimeGameObjectManager<T> manager, string assetName, Func<UniTask<T>> onCreateAsync, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) : base(onCreateAsync, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize)
            {
                m_Manager = manager;
                m_AssetName = assetName;
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

        private GameObjectPool CreateGameObjectPool(string assetPath, bool async)
        {
            Action<T> onGet = (instance) =>
            {
                instance.transform.SetParent(null);
                instance.ObjectID = ++s_GlobalObjectID;
                instance.OnPoolableGet();
            };
            Action<T> onRelease = (instance) =>
            {
                instance.gameObject.transform.SetParent(m_GameObjectPoolRoot.transform);
                instance.ObjectID = 0;
                instance.OnPoolableRelease();
            };
            Action<T> onDestroy = (instance) =>
            {
                instance.OnPoolableDestroy();
                GameObject.Destroy(instance.gameObject);
            };

            if (async)
            {
                return new GameObjectPool(this, assetPath,
                        onCreateAsync: async () =>
                        {
                            var path = PrefixPath + assetPath;
                            var prefab = await m_AssetLoader.LoadAssetRefAsync<T>(path);

                            if (prefab == null)
                            {
                                Log.ERROR($"[{nameof(RuntimeGameObjectManager<T>)}] {nameof(SpawnGameObjectAsync)} failed. path: {path}");
                                return default;
                            }

                            var instance = GameObject.Instantiate(prefab);
                            instance.AssetPath = assetPath;
                            return instance;
                        },
                        onGet: onGet,
                        onRelease: onRelease,
                        onDestroy: onDestroy
                );
            }
            else
            {
                return new GameObjectPool(this, assetPath,
                        onCreate: () =>
                        {
                            var path = PrefixPath + assetPath;
                            var prefab = m_AssetLoader.LoadAssetRef<T>(path);

                            if (prefab == null)
                            {
                                Log.ERROR($"[{this.GetType().Name}] {nameof(SpawnGameObjectAsync)} failed. path: {path}");
                                return default;
                            }

                            var instance = GameObject.Instantiate(prefab);
                            instance.AssetPath = assetPath;
                            return instance;
                        },
                        onGet: onGet,
                        onRelease: onRelease,
                        onDestroy: onDestroy
                );
            }
        }
    }
}