
namespace Saro
{
    /// <summary>
    /// C#单例 慎用!!!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : class, new()
    {
        [System.Obsolete("use \"Instance\" instead")]
        public static T Get()
        {
            return Instance;
        }

        public static T Instance { get; } = new T();
    }

    [System.Obsolete("use Singleton", true)]
    public class _Singleton<T> where T : class, new()
    {
        [System.Obsolete("use \"Instance\" instead")]
        public static T Get()
        {
            return Instance;
        }

        private static readonly object s_LockObj = new object();
        private static T s_Instance = new();

        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (s_LockObj)
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = new T();
                        }
                    }
                }
                return s_Instance;
            }
            protected set
            {
                lock (s_LockObj)
                {
                    s_Instance = value;
                }
            }
        }
    }
}

