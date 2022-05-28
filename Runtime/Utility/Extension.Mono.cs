using UnityEngine;

namespace Saro.Utility
{
    public static partial class Extension
    {
        /// <summary>
        /// 对此节点下所有 T 类型的组件进行 action 操作
        /// </summary>
        /// <param name="self"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void ForEach<T>(this MonoBehaviour self, System.Action<T> action) where T : Component
        {
            T[] components = self.GetComponentsInChildren<T>();
            for (int i = 0; i < components.Length; i++)
            {
                action(components[i]);
            }
        }

        public static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            T component = self.GetComponent<T>();
            if (component == null) component = self.AddComponent<T>();
            return component;
        }

        public static T GetOrAddComponent<T>(this MonoBehaviour self) where T : Component
        {
            T component = self.GetComponent<T>();
            if (component == null) component = self.gameObject.AddComponent<T>();
            return component;
        }

        public static T GetOrAddComponent<T>(this Transform self) where T : Component
        {
            T component = self.GetComponent<T>();
            if (component == null) component = self.gameObject.AddComponent<T>();
            return component;
        }
    }
}