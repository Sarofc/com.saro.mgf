using UnityEngine;

namespace Saro
{
    /*
     * Warning: 慎用！！！
     * 
     * Mono单例
     * 
     */
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