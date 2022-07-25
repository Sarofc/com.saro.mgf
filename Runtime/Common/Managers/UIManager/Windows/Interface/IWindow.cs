using UnityEngine;

namespace Saro.UI
{
    /// <summary>
    /// 窗口接口，IUIComponent生命周期被IWindow托管
    /// </summary>
    public interface IWindow : IComponent
    {
        /// <summary>
        /// ui层级
        /// </summary>
        EUILayer Layer { get; set; }

        /// <summary>
        /// 画布
        /// </summary>
        Canvas Canvas { get; }

        /// <summary>
        /// 父窗口
        /// </summary>
        IWindow Parent { get; }

        /// <summary>
        /// 窗体激活动画 TODO
        /// </summary>
        IAnimation OnFocusAnimation { get; }

        /// <summary>
        /// 窗体失活动画 TODO
        /// </summary>
        IAnimation OnUnFocusAnimation { get; }

        /// <summary>
        /// 设置父窗口
        /// </summary>
        /// <param name="window"></param>
        void SetParent(IWindow window);

        /// <summary>
        /// 当窗口重新获得焦点时会调用
        /// 如 2覆盖1上面，2关闭，1触发focus
        /// </summary>
        void OnFocus();

        /// <summary>
        /// 每帧更新
        /// </summary>
        void Update(float dt);

        /// <summary>
        /// 注册子窗口
        /// </summary>
        /// <param name="subwin"></param>
        void RegisterSubWindow(IWindow subwin);

        /// <summary>
        /// 获取子窗口
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <returns></returns>
        T1 GetSubWindow<T1>() where T1 : class;
    }
}