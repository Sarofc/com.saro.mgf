using Saro.Core;
using Saro.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Saro.UI
{
    public abstract class UIWindow : UIComponent, IWindow
    {
        public EUILayer Layer { get; set; }

        public override IAssetLoader AssetLoader
        {
            get
            {
                if (m_AssetLoader == null)
                {
                    m_AssetLoader = AssetLoaderFactory.Create<DefaultAssetLoader>(4, true);
                }

                return m_AssetLoader;
            }
        }

        public Canvas Canvas { get; private set; }
        public IAnimation OnFocusAnimation { get; }
        public IAnimation OnUnFocusAnimation { get; }
        public int WindowPriority { get; set; }

        private IAssetLoader m_AssetLoader;

        protected UIWindow(string resPath) : base(resPath, null)
        {
        }

        #region 生命周期

        protected override void Awake()
        {
            base.Awake();

            Canvas = Root.GetOrAddComponent<Canvas>();
            Root.GetOrAddComponent<GraphicRaycaster>();
        }

        void IWindow.OnFocus()
        {
            ((IComponent) this).Show(UserData);
        }

        protected virtual void OnUpdate(float dt)
        {
        }

        void IWindow.Update(float dt)
        {
            OnUpdate(dt);
        }

        protected override void OnDestroy()
        {
            GameObject.Destroy(this.Root.gameObject);
            this.Root = null;

            IsDestroy = true;
            IsOpen = false;

            ClearEvents();

            // 销毁时，释放掉ui对象加载的所有资源
            if (m_AssetLoader != null)
            {
                AssetLoaderFactory.Release(m_AssetLoader);
                m_AssetLoader = null;
            }
        }

        #endregion

        #region 子窗口

        /// <summary>
        /// 父节点
        /// </summary>
        public IWindow Parent { get; private set; }

        /// <summary>
        /// 子窗口列表
        /// </summary>
        protected Dictionary<int, IWindow> m_SubWindowMap = new();

        /// <summary>
        /// 注册窗口
        /// </summary>
        /// <param name="subwin"></param>
        /// <param name="enum"></param>
        public void RegisterSubWindow(IWindow subwin)
        {
            m_SubWindowMap[subwin.GetHashCode()] = subwin;
            subwin.SetParent(this);
        }

        /// <summary>
        /// 设置父节点
        /// </summary>
        /// <param name="window"></param>
        public void SetParent(IWindow window)
        {
            this.Parent = window;
        }

        /// <summary>
        /// 获取窗口
        /// </summary>
        /// <param name="enum"></param>
        /// <typeparam name="T1"></typeparam>
        /// <returns></returns>
        public T1 GetSubWindow<T1>() where T1 : class
        {
            foreach (var value in m_SubWindowMap.Values)
            {
                if (value is T1)
                {
                    return (T1) value;
                }
            }

            return null;
        }

        #endregion

        public override string ToString()
        {
            return $"{this.GetType().Name} layer: {Layer} canvas: {Canvas.overrideSorting} {Canvas.sortingOrder}";
        }
    }
}