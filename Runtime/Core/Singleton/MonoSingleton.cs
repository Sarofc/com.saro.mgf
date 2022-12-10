using UnityEngine;

namespace Saro
{
    /// <summary>
    /// Mono单例 慎用！！！
    /// <code>没有DontDestroyOnLoad，需要的话，重写 Awake 加上</code>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        public static T Instance { get; protected set; }

        protected virtual void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}