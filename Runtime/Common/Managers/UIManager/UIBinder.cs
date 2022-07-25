using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saro.UI
{
    [DisallowMultipleComponent]
    public class UIBinder : ReferenceBinder
    {
#if UNITY_EDITOR
        [SerializeField]
        private MonoScript m_UIScript;
#endif

        public bool Check()
        {
            foreach (var item in Datas)
            {
                if (item.obj == null)
                {
                    Debug.LogError("null ref value: " + item.key);
                    return false;
                }
            }

            return true;
        }
    }
}
