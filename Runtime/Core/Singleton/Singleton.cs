
namespace Saro
{

    /*
     * Warning: 慎用!!!
     *      
     * C#单例
     *
     * 需要无参构造函数
     *
     */
    public class Singleton<T> where T : class, new()
    {
        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        [System.Obsolete("use \"Instance\" instead")]
        public static T Get()
        {
            return Instance;
        }

        private static readonly object s_LockObj = new object();
        private static T s_Instance = null;

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

