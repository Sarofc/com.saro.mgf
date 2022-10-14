using Cysharp.Threading.Tasks;
using Saro.Core;
using Saro.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace Saro.Gameplay.Effect
{
    // TODO 分析需求，重构下，参考下dota2的粒子管理，或者其他项目
    public class EffectManager : IEffectManager, IService
    {
        private class EffectPool
        {
            public Stack<VfxScriptBase> effects;
            public float usedTime;

            public void Use()
            {
                usedTime = Time.realtimeSinceStartup;
            }
        }

        public static EffectManager Current => Main.Resolve<EffectManager>();

        private GameObject m_EffectRoot;
        private int m_GlobalObjectID;

        public string EffectPath { get; private set; }

        private readonly Dictionary<string, EffectPool> m_EffectMap = new Dictionary<string, EffectPool>();

        // TODO 不要用这个，用DefaultAssetLoader
        private readonly LruAssetLoader m_LruAssetLoader = AssetLoaderFactory.Create<LruAssetLoader>(128);

        public void SetLoadPath(string effectPath)
        {
            EffectPath = effectPath;
        }

        public async UniTask<ObjectHandle<VfxScriptBase>> CreateEffectAsync(string effectName, Vector3 position)
        {
            bool createNew = false;
            VfxScriptBase effect = null;
            if (m_EffectMap.TryGetValue(effectName, out var effectPool))
            {
                var pool = effectPool.effects;

                if (pool.Count > 0)
                    effect = pool.Pop();
                else
                    createNew = true;

                effectPool.Use();
            }
            else
            {
                createNew = true;
            }

            if (createNew)
            {
                var effectAssetPath = EffectPath + effectName;
                var effectPrefab = await m_LruAssetLoader.LoadAssetRefAsync<VfxScriptBase>(effectAssetPath);

                if (effectPrefab == null)
                {
                    Log.ERROR($"[EffectManager] CreateEffectAsync failed. path: {effectAssetPath}");
                    return default;
                }

                effect = GameObject.Instantiate(effectPrefab);
                effect.EffectName = effectName;
            }

            effect.transform.SetParent(null);
            effect.transform.position = position;

            effect.Init();
            effect.ObjectID = ++m_GlobalObjectID;

            return new(effect);
        }

        public ObjectHandle<VfxScriptBase> CreateEffect(string effectName)
        {
            bool createNew = false;
            VfxScriptBase effect = null;
            if (m_EffectMap.TryGetValue(effectName, out var effectPool))
            {
                var pool = effectPool.effects;

                if (pool.Count > 0)
                    effect = pool.Pop();
                else
                    createNew = true;
            }
            else
            {
                createNew = true;
            }

            if (createNew)
            {
                var effectAssetPath = EffectPath + effectName;
                var effectPrefab = m_LruAssetLoader.LoadAssetRef<VfxScriptBase>(effectAssetPath);

                if (effectPrefab == null)
                {
                    Log.ERROR($"[EffectManager] CreateEffect failed. path: {effectAssetPath}");
                    return default;
                }

                effect = GameObject.Instantiate(effectPrefab);
                effect.EffectName = effectName;
            }

            effect.Init();
            effect.ObjectID = ++m_GlobalObjectID;

            return new(effect);
        }

        public void SetEffectControlPoint(in ObjectHandle<VfxScriptBase> handle, int cpIndex, ControlPoint cp)
        {
            if (handle)
            {
                var effect = handle.Object;
                if (effect.cps != null && effect.cps.Length > cpIndex)
                {
                    effect.cps[cpIndex] = cp;
                }
                else
                {
                    Log.ERROR($"[EffectManager] SetEffectControlPoint failed. index out of range or cps is null: {cpIndex}");
                }
            }
        }

        public void SetEffectControlEntity(in ObjectHandle<VfxScriptBase> handle, int ceIndex, ControlEntity ce)
        {
            if (handle)
            {
                var effect = handle.Object;
                if (effect.ces != null && effect.ces.Length > ceIndex)
                {
                    effect.ces[ceIndex] = ce;
                }
                else
                {
                    Log.ERROR($"[EffectManager] SetEffectControlEntity failed. index out of range or ces is null: {ceIndex}");
                }
            }
        }

        public void ReleaseEffect(in ObjectHandle<VfxScriptBase> handle)
        {
            if (handle)
            {
                var effect = handle.Object;
                ReleaseEffect(effect);
            }
        }

        public void ReleaseEffect(VfxScriptBase effect)
        {
            if (effect == null)
            {
                Log.ERROR($"[EffectManager] ReleaseEffect failed. Effect is null");
                return;
            }

            if (!m_EffectMap.TryGetValue(effect.EffectName, out var effectPool))
            {
                effectPool = new EffectPool
                {
                    effects = new Stack<VfxScriptBase>(32)
                };
                m_EffectMap.Add(effect.EffectName, effectPool);
            }

            effect.Clean();
            effect.gameObject.transform.SetParent(m_EffectRoot.transform);
            effectPool.effects.Push(effect);
        }

        public void DestroyEffect(VfxScriptBase effect)
        {
        }

        public void DestroyEffects()
        {
            foreach (var item in m_EffectMap)
            {
                var effectPool = item.Value;
                var pool = effectPool.effects;
                while (pool.Count > 0)
                {
                    var effect = pool.Pop();
                    GameObject.Destroy(effect.gameObject);
                }
            }

            m_EffectMap.Clear();
        }

        void IService.Awake()
        {
            m_EffectRoot = new GameObject("EffectRoot");
            m_EffectRoot.SetActive(false);
            GameObject.DontDestroyOnLoad(m_EffectRoot);

            m_EffectRoot.transform.hierarchyCapacity = 64;
        }

        void IService.Update()
        {
            // TODO 自动清理长时间不用的特效池子
        }

        void IService.Dispose()
        {
            if (m_EffectRoot != null)
            {
                GameObject.Destroy(m_EffectRoot);
            }
        }
    }
}