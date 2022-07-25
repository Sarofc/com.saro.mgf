using Cysharp.Threading.Tasks;
using Saro.Core;
using Saro.Events;
using Saro.Pool;
using Saro.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Saro.UI
{
    public abstract class UIComponent : IComponent
    {
        public Transform Root { get; protected set; }
        public UIBinder Binder { get; protected set; }
        public virtual IAssetLoader AssetLoader { get; set; }
        public bool IsLoad { get; protected set; }
        public bool IsOpen { get; protected set; }
        public bool IsDestroy { get; protected set; }
        public object UserData { get; protected set; }
        public CanvasGroup CanvasGroup { get; protected set; }
        public IAnimation ShowAnimation { get; protected set; }
        public IAnimation HideAnimation { get; protected set; }

        protected string m_ResPath;

        #region 构造相关

        public UIComponent(IAssetLoader assetLoader)
        {
            AssetLoader = assetLoader;

            var t = this.GetType();
            var attr = t.GetCustomAttribute<UIComponentAttribute>();
            if (attr == null)
            {
                return;
            }

            m_ResPath = attr.Path;
        }

        public UIComponent(string resPath, IAssetLoader assetLoader)
        {
            this.m_ResPath = resPath;
            AssetLoader = assetLoader;
        }

        /// <summary>
        /// 此构造，直接同步完成了，无需加载资源
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="assetLoader"></param>
        public UIComponent(Transform trans, IAssetLoader assetLoader)
        {
            Root = trans;
            AssetLoader = assetLoader;

            AfterLoad();
        }

        /// <summary>
        /// 加载接口
        /// </summary>
        public void Load()
        {
            if (IsLoad) return;

            if (m_ResPath == null) return;

            var obj = AssetLoader.LoadAssetRef<GameObject>(m_ResPath);

            if (obj == null) return;

            Root = GameObject.Instantiate(obj).transform;

            AfterLoad();
        }

        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="callback"></param>
        public async UniTask<bool> LoadAsync()
        {
            if (IsLoad) return true;

            if (m_ResPath == null) return false;

            var obj = await AssetLoader.LoadAssetRefAsync<GameObject>(m_ResPath);

            if (obj == null) return false;

            Root = GameObject.Instantiate(obj).transform;

            AfterLoad();

            return true;
        }

        protected void AfterLoad()
        {
            if (IsLoad) return;

            Binder = Root.GetComponent<UIBinder>();

            IsLoad = true;

            Awake();
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// 注册事件、一些组件初始化放这里
        /// </summary>
        protected virtual void Awake()
        {
            CanvasGroup = Root.GetOrAddComponent<CanvasGroup>();

            ShowAnimation = Root.GetComponent<IShowWindowAnimation>() as IAnimation;
            if (ShowAnimation != null) ShowAnimation.SetComponent(this);
            HideAnimation = Root.GetComponent<IHideWindowAnimation>() as IAnimation;
            if (HideAnimation != null) HideAnimation.SetComponent(this);
        }

        void IComponent.Show(object userData = null, bool ignoreAnimation = false)
        {
            UserData = userData;

            Root.gameObject.SetActive(true);

            if (!ignoreAnimation && ShowAnimation != null)
            {
                ShowAnimation
                    //.OnStart(null)
                    .OnEnd(() =>
                    {
                        //OnShow();
                    })
                    .Play();
            }
            else
            {
            }

            OnShow();

            IsOpen = true;
        }

        void IComponent.Hide(bool ignoreAnimation = false)
        {
            if (!ignoreAnimation && HideAnimation != null)
            {
                HideAnimation
                    //.OnStart(null)
                    .OnEnd(() =>
                    {
                        OnHide();
                    })
                    .Play();
            }
            else
            {
                OnHide();
            }
        }

        /// <summary>
        /// Show 动画开始之前，调用
        /// </summary>
        protected virtual void OnShow()
        {
        }

        /// <summary>
        /// Hide 动画结束之后，调用
        /// </summary>
        protected virtual void OnHide()
        {
            IsOpen = false;
            Root.gameObject.SetActive(false);
        }

        void IComponent.Destroy()
        {
            OnDestroy();
        }

        protected virtual void OnDestroy()
        {
            Root = null;
            IsDestroy = true;
            IsOpen = false;

            ClearEvents();
        }

        #endregion

        #region 事件

        private List<UnityAction> Listeners => m_Listeners ?? (m_Listeners = ListPool<UnityAction>.Rent());

        private List<UnityAction> m_Listeners;

        public void Listen(UnityEvent src, UnityAction dst)
        {
            src.AddListener(dst);

            Listeners.Add(() => src.RemoveListener(dst));
        }

        public void Listen<TArg>(UnityEvent<TArg> src, UnityAction<TArg> dst)
        {
            src.AddListener(dst);

            Listeners.Add(() => src.RemoveListener(dst));
        }

        public void Listen(int eventID, EventHandler<GameEventArgs> handler)
        {
            EventManager.Global.Subscribe(eventID, handler);

            Listeners.Add(() => EventManager.Global.Unsubscribe(eventID, handler));
        }

        protected void ClearEvents()
        {
            if (m_Listeners != null)
            {
                foreach (var action in m_Listeners)
                {
                    action?.Invoke();
                }

                ListPool<UnityAction>.Return(m_Listeners);
                m_Listeners = null;
            }
        }

        #endregion
    }
}