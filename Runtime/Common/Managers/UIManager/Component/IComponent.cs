using Cysharp.Threading.Tasks;
using Saro.Core;
using UnityEngine;

namespace Saro.UI
{
    /// <summary>
    /// 组件接口
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// 资源节点
        /// </summary>
        Transform Root { get; }

        /// <summary>
        /// 绑定器
        /// </summary>
        UIBinder Binder { get; }

        /// <summary>
        /// 
        /// </summary>
        CanvasGroup CanvasGroup { get; }

        /// <summary>
        /// 资源加载器
        IAssetLoader AssetLoader { get; set; }
        /// </summary>

        /// <summary>
        /// 是否加载
        /// </summary>
        bool IsLoad { get; }

        /// <summary>
        /// 是否打开
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 是否销毁
        /// </summary>
        bool IsDestroy { get; }

        /// <summary>
        /// 用户数据，可以通过  <see cref="IComponent.Show(object, bool)"/> 传入 
        /// </summary>
        object UserData { get; }

        /// <summary>
        /// 进入动画
        /// </summary>
        IAnimation ShowAnimation { get; }

        /// <summary>
        /// 离开动画
        /// </summary>
        IAnimation HideAnimation { get; }

        /// <summary>
        /// 同步加载
        /// </summary>
        void Load();

        /// <summary>
        /// 异步加载  WARN UIManager调用，其他地方不要调用
        /// </summary>
        UniTask<bool> LoadAsync();

        /// <summary>
        /// 打开 WARN UIManager调用，其他地方不要调用
        /// </summary>
        void Show(object userData = null, bool ignoreAnimation = false);

        /// <summary>
        /// 关闭 WARN UIManager调用，其他地方不要调用
        /// </summary>
        void Hide(bool ignoreAnimation = false);

        /// <summary>
        /// 删除  WARN UIManager调用，其他地方不要调用
        /// </summary>
        void Destroy();
    }
}