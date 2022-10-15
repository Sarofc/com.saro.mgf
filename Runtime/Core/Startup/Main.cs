using Saro.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Saro.Core;
using Saro.Utility;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saro
{
    /*
     * GameApp Entry
     */
    public sealed partial class Main : MonoSingleton<Main>
    {
        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);

            SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Current);
        }

        // 考量是，丢场景里的mono脚本的Awake执行完毕后，再启动会合适一些
        private async void Start()
        {
            var startupTypes = TypeUtility.GetSubClassTypesAllAssemblies(typeof(IStartup));

            if (startupTypes != null && startupTypes.Count != 1)
            {
                throw new NullReferenceException($"MUST have only a class impl from {nameof(IStartup)}. class: {string.Join(", ", startupTypes)}");
            }

            try
            {
                var startup = Activator.CreateInstance(startupTypes[0]) as IStartup;
                await startup.StartAsync();
            }
            catch (Exception e)
            {
                Log.ERROR("Main", e);
            }
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
#endif
            Application.Quit();
        }

        private void Update()
        {
            m_Locator.Update();

            onUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            ThreadSynchronizationContext.Current.Update();

            onLateUpdate?.Invoke();
        }


        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke();
        }

        private void OnApplicationQuit()
        {
            m_Locator.Dispose();

            onApplicationQuit?.Invoke();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            onApplicationPause?.Invoke(pauseStatus);
        }

        private void OnApplicationFocus(bool focus)
        {
            onApplicationFocus?.Invoke(focus);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            onUpdate = null;
            onFixedUpdate = null;
            onLateUpdate = null;

            onGUI = null;

#if DEBUG
            onDrawGizmos = null;
#endif
        }

        private void OnGUI()
        {
            onGUI?.Invoke();
        }

#if DEBUG
        private void OnDrawGizmos()
        {
            onDrawGizmos?.Invoke();
        }
#endif
    }

    /*
     * IServiceLocator
     */
    public partial class Main
    {
        private IServiceLocator m_Locator = new DefaultServiceLocator();

        public static void SetServiceLocator(IServiceLocator locator)
        {
            Instance.m_Locator = locator;
        }

        public static T Register<T>() where T : class, IService, new()
        {
            return Instance.m_Locator.Register<T>();
        }

        public static T Register<T>(T service) where T : class, IService
        {
            return Instance.m_Locator.Register<T>(service);
        }

        public static T Resolve<T>() where T : class, IService
        {
            return Instance.m_Locator.Resolve<T>();
        }

        public static IService Register(Type type)
        {
            return Instance.m_Locator.Register(type);
        }

        public static IService Register(Type type, IService service)
        {
            return Instance.m_Locator.Register(type, service);
        }

        public static IService Resolve(Type type)
        {
            return Instance.m_Locator.Resolve(type);
        }
    }

    /*
     * AssetLoader
     */
    public partial class Main
    {
        /// <summary>
        /// 全局 资源加载器
        /// <code>默认使用 <see cref="DefaultAssetLoader"/>，资源不会自动释放。可自行更换 <see cref="SetMainAssetLoader(IAssetLoader)"/></code>
        /// </summary>
        public static IAssetLoader MainAssetLoader => Instance.m_MainAssetLoader;

        private IAssetLoader m_MainAssetLoader = AssetLoaderFactory.Create<DefaultAssetLoader>(2048, false);

        /// <summary>
        /// 设置全局资源管理器
        /// </summary>
        /// <param name="loader"></param>
        public static void SetMainAssetLoader(IAssetLoader loader)
        {
            Instance.m_MainAssetLoader = loader;
        }
    }

    /*
     * MonoBehaviour Event Function
     */
    public partial class Main
    {
        #region Mono LifeCycle

        /// <summary>
        /// Mono Update 回调
        /// </summary>
        public static FDelegates onUpdate = new();

        /// <summary>
        /// Mono LateUpdate 回调
        /// </summary>
        public static FDelegates onLateUpdate = new();

        /// <summary>
        /// Mono FixedUpdate 回调
        /// </summary>
        public static FDelegates onFixedUpdate = new();

        /// <summary>
        /// Mono OnApplicationPause 回调
        /// </summary>
        public static FDelegates<bool> onApplicationPause = new();

        /// <summary>
        /// Mono OnApplicationFocus 回调
        /// </summary>
        public static FDelegates<bool> onApplicationFocus = new();

        /// <summary>
        /// Mono OnApplicationQuit 回调
        /// </summary>
        public static FDelegates onApplicationQuit = new();

        private static event Action onGUI;

#if DEBUG
        private static event Action onDrawGizmos;
#endif

        /// <summary>
        /// [DEBUG] 注册 Mono OnGUI 回调
        /// </summary>
        //[System.Diagnostics.Conditional("DEBUG")]
        public static void AddOnGUIListener(Action guiDelegate)
        {
            onGUI += guiDelegate;
        }

        /// <summary>
        /// [DEBUG] 反注册 Mono OnGUI 回调
        /// </summary>
        //[System.Diagnostics.Conditional("DEBUG")]
        public static void RemoveOnGUIListener(Action guiDelegate)
        {
            onGUI -= guiDelegate;
        }

        /// <summary>
        /// [DEBUG] 注册 Mono OnGUI 回调
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void AddOnDraGizmosListener(Action guiDelegate)
        {
#if DEBUG
            onDrawGizmos += guiDelegate;
#endif
        }

        /// <summary>
        /// [DEBUG] 反注册 Mono OnGUI 回调
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void RemoveOnDraGizmosListener(Action guiDelegate)
        {
#if DEBUG
            onDrawGizmos -= guiDelegate;
#endif
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public void DumpMonoCallbacks()
        {
#if DEBUG
            var sb = StringBuilderCache.Get(1024);
            int counter = 0;
            if (onUpdate != null)
            {
                sb.AppendLine("<color=red>Update Delegates: </color>");
                foreach (var item in onUpdate.RuntimeCalls)
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onLateUpdate != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>LateUpdate Delegates: </color>");
                foreach (var item in onLateUpdate.RuntimeCalls)
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onFixedUpdate != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>FixedUpdate Delegates: </color>");
                foreach (var item in onFixedUpdate.RuntimeCalls)
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onApplicationPause != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>OnApplicationPause Delegates: </color>");
                foreach (var item in onApplicationPause.RuntimeCalls)
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onApplicationFocus != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>OnApplicationFocus Delegates: </color>");
                foreach (var item in onApplicationFocus.RuntimeCalls)
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onApplicationQuit != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>OnApplicationQuit Delegates: </color>");
                foreach (var item in onApplicationQuit.RuntimeCalls)
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onGUI != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>OnGUI Delegates: </color>");
                foreach (var item in onGUI.GetInvocationList())
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onDrawGizmos != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>OnDrawGizmos Delegates: </color>");
                foreach (var item in onDrawGizmos.GetInvocationList())
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            print(StringBuilderCache.GetStringAndRelease(sb));

#endif
        }

        #endregion
    }

    /*
     * Coroutine Support
     */
    public partial class Main
    {
        #region Coroutine

        private TMultiMap<object, IEnumerator> m_Coroutines = new TMultiMap<object, IEnumerator>();

        /// <summary>
        /// 开启协程
        /// </summary>
        /// <param name="routine">协程方法</param>
        /// <param name="objectRef">协程Key，用于获取到对应的协程对象</param>
        /// <returns></returns>
        public static Coroutine RunCoroutine(IEnumerator routine, object objectRef = null)
        {
            Instance.m_Coroutines = new TMultiMap<object, IEnumerator>();

            if (objectRef != null)
            {
                Instance.m_Coroutines.Add(objectRef, routine);
            }

            return Instance.StartCoroutine(routine);
        }

        /// <summary>
        /// 取消协程
        /// </summary>
        /// <param name="routine"></param>
        public static void CancelCoroutine(Coroutine routine)
        {
            Instance.StopCoroutine(routine);
        }

        /// <summary>
        /// 取消协程
        /// </summary>
        /// <param name="routine"></param>
        public static void CancelCoroutine(IEnumerator routine)
        {
            Instance.StopCoroutine(routine);
        }

        /// <summary>
        /// 根据key取消对应所有协程
        /// </summary>
        /// <param name="objectRef"></param>
        public static void CancelAllCoroutinesWithObjectRef(object objectRef)
        {
            if (Instance.m_Coroutines.TryGetValue(objectRef, out TLinkedListRange<IEnumerator> range))
            {
                LinkedListNode<IEnumerator> current = range.Head;
                while (current != null && current != range.Tail)
                {
                    Instance.StopCoroutine(current.Value);
                    current = current.Next != range.Tail ? current.Next : null;
                }

                Instance.m_Coroutines.RemoveAll(objectRef);
            }
        }

        /// <summary>
        /// 取消所有协程
        /// </summary>
        public static void CancelAllCoroutines()
        {
            Instance.StopAllCoroutines();

            Instance.m_Coroutines.Clear();
        }

        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(Main))]
    public class MainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button($"{nameof(Main.DumpMonoCallbacks)}"))
            {
                Main.Instance.DumpMonoCallbacks();
            }

            if (GUILayout.Button($"{nameof(TestMonoCallbacks)}"))
            {
                TestMonoCallbacks();
            }
        }

        private void TestMonoCallbacks()
        {
            Main.onUpdate += () => Log.ERROR("onUpdate");
            Main.onFixedUpdate += () => Log.ERROR("onFixedUpdate");
            Main.onLateUpdate += () => Log.ERROR("onLateUpdate");
            Main.onApplicationFocus += (val) => Log.ERROR($"onApplicationFocus: {val}");
            Main.onApplicationPause += (val) => Log.ERROR($"onApplicationPause: {val}");
            Main.onApplicationQuit += () => Log.ERROR("onApplicationQuit");
        }
    }
#endif
}