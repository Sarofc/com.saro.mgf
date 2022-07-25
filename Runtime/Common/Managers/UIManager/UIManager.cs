using Cysharp.Threading.Tasks;
using Saro.Collections;
using Saro.Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Saro.UI
{
    /*
     * TODO 
     * 
     * 继续总结ui框架，这个实现的不怎么样
     * 
     * 1. 抽象窗体，覆盖大多数业务逻辑
     * 2. api设计，简单易用
     * 3. 层级管理
     * 4. 特效管理
     * 5. ui窗体缓存
     * 6. ui脚本绑定
     * 7. ui事件
     * 8. 接入huatuo
     * 
     */

    public enum EUILayer
    {
        Bottom = 1000,
        Center = 4000,
        Top = 8000,
    }

    /// <summary>
    /// UI管理类
    /// </summary>
    public partial class UIManager : IService
    {
        public static UIManager Current => Main.Resolve<UIManager>();

        [System.Obsolete("use ‘Current’ instead")]
        public static UIManager Instance => Current;

        /// <summary>
        /// UI窗口字典
        /// </summary>
        private Dictionary<int, IWindow> m_WindowMap = null;

        /// <summary>
        /// ui的三个层级
        /// </summary>
        public Transform Bottom { get; private set; }
        public Transform Center { get; private set; }
        public Transform Top { get; private set; }
        internal GameObject Mask { get; private set; }

        void IService.Awake()
        {
            Init();
        }

        void IService.Update()
        {
            float dt = Time.deltaTime;

            for (int i = 0; i < m_BottomUIs.Count; i++)
            {
                var win = GetWindow(m_BottomUIs[i]);
                if (win.IsOpen)
                {
                    win.Update(dt);
                }
            }

            for (int i = 0; i < m_CenterUIs.Count; i++)
            {
                var win = GetWindow(m_CenterUIs[i]);
                if (win.IsOpen)
                {
                    win.Update(dt);
                }
            }

            for (int i = 0; i < m_TopUIs.Count; i++)
            {
                var win = GetWindow(m_TopUIs[i]);
                if (win.IsOpen)
                {
                    win.Update(dt);
                }
            }
        }

        void IService.Dispose()
        {
        }

        private void Init()
        {
            CacheUIAttributes();

            //初始化
            m_WindowMap = new Dictionary<int, IWindow>();

            try
            {
                var uiroot = GameObject.Find("[UIRoot]").transform;
                if (uiroot)
                {
                    Bottom = uiroot.Find("Bottom").transform;
                    Center = uiroot.Find("Center").transform;
                    Top = uiroot.Find("Top").transform;
                    Mask = uiroot.Find("[Mask]").gameObject;
                    //不删除uiroot
                    if (Application.isPlaying)
                    {
                        GameObject.DontDestroyOnLoad(uiroot.gameObject);
                    }
                }
            }
            catch (Exception e)
            {
                Log.ERROR(e);
            }
        }

        private Dictionary<int, UIWindowAttribute> m_UIDefCache = new();
        private void CacheUIAttributes()
        {
            var asmMap = Saro.Utility.ReflectionUtility.AssemblyMap;
            foreach (var asm in asmMap)
            {
                var types = asm.Value.GetTypes();
                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute<UIWindowAttribute>();
                    if (attr == null) continue;
                    attr.Type = type;
                    m_UIDefCache.Add(attr.Index, attr);
                }
            }
        }

        /// <summary>
        /// 创建一个窗口
        /// </summary>
        /// <param name="uiIdx"></param>
        /// <returns></returns>
        private IWindow CreateWindow(int uiIdx)
        {
            if (!this.m_UIDefCache.TryGetValue(uiIdx, out var uiAttr))
            {
                Debug.LogError("未注册窗口，无法加载:" + uiIdx);
                return null;
            }

            //根据attribute创建窗口
            var window = Activator.CreateInstance(uiAttr.Type, new object[] { uiAttr.AssetPath }) as IWindow;

            return window;
        }

        #region 资源相关处理

        /// <summary>
        /// 加载窗口
        /// </summary>
        /// <param name="uiIndexs">窗口枚举</param>
        public void LoadWindow(Enum uiIndex)
        {
            var index = uiIndex.GetHashCode();

            if (m_WindowMap.ContainsKey(index))
            {
                var com = m_WindowMap[index] as IComponent;
                if (com.IsLoad)
                {
                    Debug.Log("已经加载过并未卸载" + index);
                }
            }
            else
            {
                //创建ui
                var com = CreateWindow(index) as IComponent;
                if (com == null)
                {
                    Debug.Log("不存在UI:" + index);
                }
                else
                {
                    m_WindowMap[index] = com as IWindow;
                    com.Load();
                    com.Root.gameObject.SetActive(false);
                    com.Root.SetParent(this.Bottom, false);
                }
            }
        }

        /// <summary>
        /// 加载窗口
        /// </summary>
        /// <param name="uiIndexs">窗口枚举</param>
        public async UniTask LoadWindowAsync(Enum uiIndex)
        {
            //var index = uiIndex.GetHashCode();

            //if (windowMap.ContainsKey(index))
            //{
            //    var uvalue = windowMap[index] as IComponent;
            //    if (uvalue.IsLoad)
            //    {
            //        Debug.Log("已经加载过并未卸载" + index);
            //    }
            //}
            //else
            //{
            //    //创建ui
            //    var window = CreateWindow(index) as IComponent;
            //    if (window == null)
            //    {
            //        Debug.Log("不存在UI:" + index);
            //    }
            //    else
            //    {
            //        windowMap[index] = window as IWindow;

            //        // TODO bug 如果第二次进来，字典已存在，依然没有加载完成，就会报空
            //        //开始窗口加载
            //        var result = await window.LoadAsync();
            //        if (result)
            //        {
            //            if (window.Root)
            //            {
            //                window.Root.gameObject.SetActive(false);
            //                window.Root.SetParent(this.Bottom, false);
            //            }
            //        }
            //    }
            //}

            var index = uiIndex.GetHashCode();

            if (!m_WindowMap.TryGetValue(index, out var window))
            {
                //创建ui
                window = CreateWindow(index);
                if (window == null)
                {
                    Debug.Log("不存在UI:" + index);
                }
                else
                {
                    m_WindowMap[index] = window as IWindow;
                }
            }

            if (!window.IsLoad)
            {
                //开始窗口加载
                var result = await window.LoadAsync();
                if (result)
                {
                    if (window.Root)
                    {
                        window.Root.gameObject.SetActive(false);
                        window.Root.SetParent(this.Bottom, false);
                    }
                }
                else
                {
                    Log.ERROR($"window.LoadAsync() failed.");
                }
            }
        }

        /// <summary>
        /// 卸载窗口
        /// </summary>
        /// <param name="idxs">窗口枚举</param>
        public void UnLoadWindows(List<Enum> idxs)
        {
            foreach (var i in idxs)
            {
                UnLoadWindow(i);
            }
        }

        /// <summary>
        /// 卸载窗口
        /// </summary>
        /// <param name="indexs">窗口枚举</param>
        public void UnLoadWindow(Enum uiIdx)
        {
            var idx = uiIdx.GetHashCode();
            if (m_WindowMap.ContainsKey(idx))
            {
                var win = m_WindowMap[idx];
                var winCom = win as IComponent;
                //winCom.Close();
                winCom.Destroy();

                RemoveFromUIList(idx, win.Layer);

                m_WindowMap.Remove(idx);
            }
            else
            {
                Debug.LogErrorFormat("不存在UI：{0}", idx);
            }
        }

        #endregion

        #region 打开、关闭

        public T LoadAndShowWindow<T>(Enum uiEnumIdx, object userData = null, EUILayer layer = EUILayer.Center, bool isAddToHistory = true) where T : class, IWindow
        {
            return LoadAndShowWindow(uiEnumIdx, userData, layer, isAddToHistory) as T;
        }

        public async UniTask<T> LoadAndShowWindowAsync<T>(Enum uiEnumIdx, object userData = null, EUILayer layer = EUILayer.Center, bool isAddToHistory = true) where T : class, IWindow
        {
            return await LoadAndShowWindowAsync(uiEnumIdx, userData, layer, isAddToHistory) as T;
        }

        public IWindow LoadAndShowWindow(Enum uiEnumIdx, object userData = null, EUILayer layer = EUILayer.Center, bool isAddToHistory = true)
        {
            LoadWindow(uiEnumIdx);
            return ShowWindow(uiEnumIdx, userData, layer, isAddToHistory);
        }

        public async UniTask<IWindow> LoadAndShowWindowAsync(Enum uiEnumIdx, object userData = null, EUILayer layer = EUILayer.Center, bool isAddToHistory = true)
        {
            Mask.SetActive(true);
            await LoadWindowAsync(uiEnumIdx);
            Mask.SetActive(false);

            return ShowWindow(uiEnumIdx, userData, layer, isAddToHistory);
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="uiIndex">窗口枚举</param>
        public T ShowWindow<T>(Enum uiEnumIdx, object userData = null, EUILayer layer = EUILayer.Center, bool isAddToHistory = true) where T : class, IWindow
        {
            int uiIdx = uiEnumIdx.GetHashCode();

            return ShowWindow(uiIdx, userData, layer, isAddToHistory) as T;
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="uiIndex">窗口枚举</param>
        public IWindow ShowWindow(Enum uiEnumIdx, object userData = null, EUILayer layer = EUILayer.Center, bool isAddToHistory = true)
        {
            int uiIdx = uiEnumIdx.GetHashCode();

            return ShowWindow(uiIdx, userData, layer, isAddToHistory);
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="uiIdx"></param>
        /// <param name="userData"></param>
        /// <param name="layer"></param>
        /// <param name="isAddToHistory"></param>
        private IWindow ShowWindow(int uiIdx, object userData = null, EUILayer layer = EUILayer.Center, bool isAddToHistory = true)
        {
            if (m_WindowMap.TryGetValue(uiIdx, out var win))
            {
                var winCom = win as IComponent;

                if (!winCom.IsOpen && winCom.IsLoad)
                {
                    win.Layer = layer;
                    var root = GetRootByLayer(layer);
                    winCom.Root.SetParent(root, false);

                    winCom.Root.SetAsLastSibling();

                    win.Show(userData);

                    AddToUIList(uiIdx, layer);

                    //effect
                }
                else
                {
                    Debug.LogFormat("UI处于[unload,lock,open]状态之一：{0}", uiIdx);
                }

                if (isAddToHistory)
                {
                    AddToHistory(uiIdx);
                }
            }
            else
            {
                Debug.LogErrorFormat("未加载UI：{0}", uiIdx);
            }

            return win; ;
        }


        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="uiIndex">窗口枚举</param>
        public void HideWindow(Enum uiEnumIdx)
        {
            var idx = uiEnumIdx.GetHashCode();

            HideWindow(idx);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="uiIdx"></param>
        public void HideWindow(int uiIdx)
        {
            if (m_WindowMap.ContainsKey(uiIdx))
            {
                var win = m_WindowMap[uiIdx];
                var winCom = win as IComponent;
                if (winCom.IsOpen && winCom.IsLoad)
                {
                    win.Hide();

                    DequeueHistroy();
                }
                else
                {
                    Debug.LogErrorFormat("UI未加载或已经处于close状态：{0}", uiIdx);
                }

                RemoveFromUIList(uiIdx, win.Layer);
            }
            else
            {
                Debug.LogErrorFormat("不存在UI：{0}", uiIdx);
            }
        }

        #endregion

        #region 导航前进、后退

        /// <summary>
        /// 当前导航UI
        /// </summary>
        private int m_CurForwardBackUI = -1;

        /// <summary>
        /// 当前导航下标
        /// </summary>
        private int m_CurForwardBackUIIdx = 0;

        /// <summary>
        /// 往前
        /// </summary>
        public void Forward()
        {
            var uiIdex = HistoryList[m_CurForwardBackUIIdx];
            //说明列表发生了变化,重设到栈顶
            if (m_CurForwardBackUI != uiIdex)
            {
                m_CurForwardBackUIIdx = HistoryList.Count - 1;
                m_CurForwardBackUI = HistoryList[m_CurForwardBackUIIdx];
            }

            if (m_CurForwardBackUIIdx < HistoryList.Count - 1)
            {
                m_CurForwardBackUIIdx++;
                m_CurForwardBackUI = this.HistoryList[m_CurForwardBackUIIdx];
                this.ShowWindow(m_CurForwardBackUI, isAddToHistory: false);
            }
            else
            {
                Debug.LogError("已经是顶部");
            }
        }

        /// <summary>
        /// 后退
        /// </summary>
        public void Back()
        {
            var uiIdex = HistoryList[m_CurForwardBackUIIdx];
            //说明列表发生了变化,重设到栈顶
            if (m_CurForwardBackUI != uiIdex)
            {
                m_CurForwardBackUIIdx = HistoryList.Count - 1;
                m_CurForwardBackUI = HistoryList[m_CurForwardBackUIIdx];
            }

            if (m_CurForwardBackUIIdx > 0)
            {
                m_CurForwardBackUIIdx++;
                m_CurForwardBackUI = this.HistoryList[m_CurForwardBackUIIdx];
                this.ShowWindow(m_CurForwardBackUI, isAddToHistory: false);
            }
            else
            {
                Debug.LogError("已经是底部");
            }
        }

        #endregion

        #region 窗口层级

        private List<int> m_BottomUIs = new();
        private List<int> m_CenterUIs = new();
        private List<int> m_TopUIs = new();

        private List<int> GetUIListByLayer(EUILayer layer)
        {
            switch (layer)
            {
                case EUILayer.Bottom:
                    return m_BottomUIs;
                case EUILayer.Center:
                    return m_CenterUIs;
                case EUILayer.Top:
                    return m_TopUIs;
            }

            throw new NotSupportedException($"{layer} is not support");
        }

        private Transform GetRootByLayer(EUILayer layer)
        {
            switch (layer)
            {
                case EUILayer.Bottom:
                    return Bottom;
                case EUILayer.Center:
                    return Center;
                case EUILayer.Top:
                    return Top;
            }

            throw new NotSupportedException($"{layer} is not support");
        }

        // 1. ui SetAsLastSibling
        // 2. 再根据canvas层级排序
        private void SortUICanvasOrder(List<int> list, EUILayer layer)
        {
            if (list == null)
            {
                return;
            }
            int order = (int)layer;
            for (int i = 0; i < list.Count; i++)
            {
                var uiIdx = list[i];

                m_WindowMap.TryGetValue(uiIdx, out var win);

                if (win == null /*|| !ui.IsOpen || ui.IsDestroy*/)
                {
                    continue;
                }

                var uiCanvas = win.Canvas;

                // reset order of invisible window
                if (!win.IsOpen)
                {
                    continue;
                }

                // dropdown会创建canvas，order是30000
                if (uiCanvas.sortingOrder > 20000)
                {
                    Log.WARN($"[{uiCanvas.name}] sortingOrder too big");
                    continue;
                }

                // overrideSorting 有bug，物体未激活时，无法赋值，且调试直接闪退
                uiCanvas.overrideSorting = true;
                uiCanvas.sortingOrder = order;

                using (ListPool<UICustomOrder>.Rent(out var customOrderList))
                {
                    // TODO 如果太耗，就editor做一层cache，保存在uibinder里
                    win.Root.GetComponentsInChildren(true, customOrderList);

                    for (int j = 0; j < customOrderList.Count; j++)
                    {
                        var customOrder = customOrderList[j];
                        if (customOrder == null)
                        {
                            continue;
                        }

                        customOrder.SetParentCanvas(uiCanvas);
                        customOrder.UpdateOrder();
                    }
                }

                order = order / 100 * 100 + 100;
            }

            //Log.INFO($"<color=green>{layer} {string.Join("\n", list.Select((idx) => GetWindow(idx)))}</color>");
        }

        private void AddToUIList(int uiIdx, EUILayer layer)
        {
            var uiList = GetUIListByLayer(layer);
            if (uiList != null)
            {
                uiList.Add(uiIdx);
                SortUICanvasOrder(uiList, layer);
            }

            //Log.INFO($"AddToUIList: {uiList.Count}");
        }

        private void RemoveFromUIList(int uiIdx, EUILayer layer)
        {
            var uiList = GetUIListByLayer(layer);
            if (uiList != null)
            {
                uiList.Remove(uiIdx);

                // 移除时，可以不用对canvas排序
                //SortUICanvasOrder(uiList, layer);
            }

            //Log.INFO($"RemoveFromUIList: {uiList.Count}");
        }

        #endregion

        #region 窗口历史

        private static int MAX_HISTORY_NUM = 50;

        /// <summary>
        /// 历史列表
        /// 永远不会重复
        /// </summary>
        public List<int> HistoryList { get; private set; } = new(MAX_HISTORY_NUM);

        /// <summary>
        /// 添加到历史列表
        /// </summary>
        /// <param name="uiIdx"></param>
        private void AddToHistory(int uiIdx)
        {
            if (HistoryList.Count == MAX_HISTORY_NUM)
            {
                HistoryList.RemoveAt(0);
            }

            //保证不会有重复列表
            HistoryList.Remove(uiIdx);
            HistoryList.Add(uiIdx);
        }

        /// <summary>
        /// 当窗口关闭
        /// </summary>
        private void DequeueHistroy(/*int uiIdx, IWindow window*/)
        {
            if (HistoryList.Count > 2)
            {
                bool isCheckFocus = false;
                for (int i = HistoryList.Count - 1; i >= 0; i--)
                {
                    var idx = HistoryList[i];
                    this.m_WindowMap.TryGetValue(idx, out var win);
                    if (win == null)
                    {
                        continue;
                    }
                    win = this.m_WindowMap[idx];
                    var winCom = win as IComponent;
                    //判断栈顶是否有关闭的,有则继续搜索第一个打开的执行focus，
                    if (!winCom.IsOpen)
                    {
                        isCheckFocus = true;
                    }
                    else if (winCom.IsOpen && isCheckFocus)
                    {
                        win.OnFocus();
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        #endregion

        #region 窗口队列

        private class QueuedInfo : IComparable<QueuedInfo>
        {
            public Enum uiIdx;
            public int priority;
            public object useData;
            public EUILayer layer;
            public bool isAddToHistory;

            public int CompareTo(QueuedInfo other)
            {
                return other.priority - priority;
            }
        }

        private readonly TBinaryHeap<QueuedInfo> m_UIPopQueue = new();
        private readonly TBinaryHeap<QueuedInfo> m_UIPopQueueAsync = new();

        private IWindow m_CurrentPopWin;

        private Coroutine m_UIPopQueueCoroutine;

        public void Queue(Enum uiIdx, int priority, object userData = null, EUILayer layer = EUILayer.Center, bool isAddToHistory = false)
        {
            m_UIPopQueue.Push(new QueuedInfo
            {
                uiIdx = uiIdx,
                priority = priority,
                useData = userData,
                layer = layer,
                isAddToHistory = isAddToHistory,
            });

            if (m_UIPopQueueCoroutine == null)
            {
                m_UIPopQueueCoroutine = Main.RunCoroutine(UpdatePopQueuedUI());
            }
        }

        public void QueueAsync(Enum uiIdx, int priority, object userData = null, EUILayer layer = EUILayer.Center, bool isAddToHistory = false)
        {
            m_UIPopQueueAsync.Push(new QueuedInfo
            {
                uiIdx = uiIdx,
                priority = priority,
                useData = userData,
                layer = layer,
                isAddToHistory = isAddToHistory,
            });

            if (m_UIPopQueueCoroutine == null)
            {
                m_UIPopQueueCoroutine = Main.RunCoroutine(UpdatePopQueuedUI());
            }
        }

        public void ClearQueue()
        {
            if (m_UIPopQueueCoroutine != null)
            {
                Main.CancelCoroutine(m_UIPopQueueCoroutine);
                m_UIPopQueueCoroutine = null;
            }

            m_UIPopQueue.Clear();
            m_UIPopQueueAsync.Clear();
        }

        private IEnumerator UpdatePopQueuedUI()
        {
            yield return null;

            while (true)
            {
                if (m_CurrentPopWin != null)
                {
                    while (m_CurrentPopWin.IsOpen)
                    {
                        yield return null;
                    }

                    m_CurrentPopWin = null;
                }

                if (m_CurrentPopWin == null)
                {
                    if (m_UIPopQueue.Count > 0)
                    {
                        var info = m_UIPopQueue.Pop();
                        LoadAndShowWindow(info.uiIdx, info.useData, info.layer, info.isAddToHistory);

                        if (m_WindowMap.TryGetValue(info.uiIdx.GetHashCode(), out var win))
                        {
                            m_CurrentPopWin = win;
                        }
                    }
                    else if (m_UIPopQueueAsync.Count > 0)
                    {
                        var info = m_UIPopQueueAsync.Pop();
                        yield return LoadAndShowWindowAsync(info.uiIdx, info.useData, info.layer, info.isAddToHistory);

                        if (m_WindowMap.TryGetValue(info.uiIdx.GetHashCode(), out var win))
                        {
                            m_CurrentPopWin = win;
                        }
                    }
                }

                if (m_CurrentPopWin == null)
                {
                    yield return null;
                }
            }
        }

        #endregion

        #region 对外接口

        /// <summary>
        /// 获取一个窗口
        /// </summary>
        /// <param name="uiIndex"></param>
        /// <returns></returns>
        public IWindow GetWindow(Enum uiIndex)
        {
            var index = uiIndex.GetHashCode();
            IWindow win = null;
            this.m_WindowMap.TryGetValue(index, out win);
            return win;
        }

        public IWindow GetWindow(int uiIdx)
        {
            IWindow win = null;
            this.m_WindowMap.TryGetValue(uiIdx, out win);
            return win;
        }

        #endregion
    }
}
