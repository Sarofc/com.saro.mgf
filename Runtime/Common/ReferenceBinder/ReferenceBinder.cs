using System;
using System.Collections.Generic;
using UnityEngine;

using UObject = UnityEngine.Object;

namespace Saro
{
    public class ReferenceBinder : MonoBehaviour
    {
        [Serializable]
        public class ReferenceBinderData
        {
            public string key;
            public UObject obj;
        }

        public class ReferenceCollectorDataComparer : IComparer<ReferenceBinderData>
        {
            public int Compare(ReferenceBinderData x, ReferenceBinderData y)
            {
                return string.Compare(x.key, y.key, StringComparison.Ordinal);
            }
        }

        public IReadOnlyList<ReferenceBinderData> Datas => m_Datas;

        [SerializeField]
        private List<ReferenceBinderData> m_Datas = new();

        protected Dictionary<string, UObject> DataMap
        {
            get
            {
                if (m_DataMap == null)
                {
                    m_DataMap = new(m_Datas.Count, StringComparer.Ordinal);

                    foreach (var data in m_Datas)
                    {
                        if (!m_DataMap.ContainsKey(data.key))
                        {
                            m_DataMap.Add(data.key, data.obj);
                        }
                    }
                }
                return m_DataMap;
            }
        }
        private Dictionary<string, UObject> m_DataMap;

        public T GetRef<T>(string key) where T : UObject
        {
            return GetRef(key) as T;
        }

        public UObject GetRef(string key)
        {
            if (!DataMap.TryGetValue(key, out UObject go))
                return null;

            return go;
        }

        public void Add(string key, UObject obj)
        {
            m_Datas.Add(new ReferenceBinderData
            {
                key = key,
                obj = obj,
            });
        }

        public void Del(string key)
        {
            for (int i = m_Datas.Count - 1; i >= 0; i--)
            {
                ReferenceBinderData data = m_Datas[i];

                if (data.key == key)
                    m_Datas.RemoveAt(i);
            }
        }

        public void DelNull()
        {
            for (int i = m_Datas.Count - 1; i >= 0; i--)
            {
                var data = m_Datas[i];
                if (data.obj == null) m_Datas.RemoveAt(i);
            }
        }

        public void DelAll()
        {
            m_Datas.Clear();
        }

        public void Sort()
        {
            m_Datas.Sort(new ReferenceCollectorDataComparer());
        }
    }
}