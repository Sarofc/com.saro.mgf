using Saro.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saro
{
    public sealed partial class Main : MonoSingleton<Main>
    {
        protected override void Awake()
        {
            base.Awake();

            try
            {
                SynchronizationContext.SetSynchronizationContext(FGame.ThreadSynchronizationContext);

                DontDestroyOnLoad(gameObject);

                // TODO config
                string[] assemblyNames = { "Assembly-CSharp", "Saro.MGF", "Saro.XAsset", "Saro.Gameplay" };

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string assemblyName = assembly.GetName().Name;
                    if (!assemblyNames.Contains(assemblyName))
                    {
                        continue;
                    }
                    FGame.EventSystem.Add(assembly);
                }

                FGame.s_Options = new Options();

                FGame.EventSystem.Publish(new EventDef.AppStart()).Forget(); // 看情况是否改成 异步等待形式
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
            FGame.Update();

            onUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            FGame.LateUpdate();

            onLateUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            onFixedUpdate?.Invoke();
        }

        private void OnApplicationQuit()
        {
            onApplicationQuit?.Invoke();

            FGame.Close();
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

#if DEBUG
            onGUI = null;
            onDrawGizmos = null;
#endif
        }

#if DEBUG
        private void OnGUI()
        {
            onGUI?.Invoke();
        }

        private void OnDrawGizmos()
        {
            onDrawGizmos?.Invoke();
        }
#endif
    }

    public sealed partial class Main
    {
        #region Mono LifeCycle

        /// <summary>
        /// Mono Update 回调
        /// </summary>
        public static event Action onUpdate;

        /// <summary>
        /// Mono LateUpdate 回调
        /// </summary>
        public static event Action onLateUpdate;

        /// <summary>
        /// Mono FixedUpdate 回调
        /// </summary>
        public static event Action onFixedUpdate;

        /// <summary>
        /// Mono OnApplicationPause 回调
        /// </summary>
        public static event Action<bool> onApplicationPause;

        /// <summary>
        /// Mono OnApplicationFocus 回调
        /// </summary>
        public static event Action<bool> onApplicationFocus;

        /// <summary>
        /// Mono OnApplicationQuit 回调
        /// </summary>
        public static event Action onApplicationQuit;

#if DEBUG
        private static event Action onGUI;
        private static event Action onDrawGizmos;
#endif

        /// <summary>
        /// [DEBUG] 注册 Mono OnGUI 回调
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void AddOnGUIListener(Action guiDelegate)
        {
#if DEBUG
            onGUI += guiDelegate;
#endif
        }

        /// <summary>
        /// [DEBUG] 反注册 Mono OnGUI 回调
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void RemoveOnGUIListener(Action guiDelegate)
        {
#if DEBUG
            onGUI -= guiDelegate;
#endif
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
                foreach (var item in onUpdate.GetInvocationList())
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onLateUpdate != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>LateUpdate Delegates: </color>");
                foreach (var item in onLateUpdate.GetInvocationList())
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onFixedUpdate != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>FixedUpdate Delegates: </color>");
                foreach (var item in onFixedUpdate.GetInvocationList())
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onApplicationPause != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>OnApplicationPause Delegates: </color>");
                foreach (var item in onApplicationPause.GetInvocationList())
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onApplicationFocus != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>OnApplicationFocus Delegates: </color>");
                foreach (var item in onApplicationFocus.GetInvocationList())
                {
                    sb.AppendFormat("  {0,-10}{1}.{2}", ++counter, item.Target?.ToString(), item.Method?.Name).AppendLine();
                }
            }

            if (onApplicationQuit != null)
            {
                counter = 0;
                sb.AppendLine("<color=red>OnApplicationQuit Delegates: </color>");
                foreach (var item in onApplicationQuit.GetInvocationList())
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

    public sealed partial class Main
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
        public static void CancelAllCoroutinesWhithObjectRef(object objectRef)
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
            // TODO 放一些配置在这里？
            base.OnInspectorGUI();
            if (GUILayout.Button($"{nameof(Main.DumpMonoCallbacks)}"))
            {
                (target as Main).DumpMonoCallbacks();
            }

            if (GUILayout.Button($"Dump {nameof(FGame)}"))
            {
                FGame.DumpEntityTree();
            }

            if (GUILayout.Button($"Dump {nameof(FEventSystem)}"))
            {
                UnityEngine.Debug.LogError(FEventSystem.Get().ToString());
            }
        }
    }
#endif
}
