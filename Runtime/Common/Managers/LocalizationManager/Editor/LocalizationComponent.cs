#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saro.Localization
{
    internal partial class LocalizationComponent
    {
        private static void SetupForLocalization<T>(UIBehaviour target) where T : MonoBehaviour
        {
            Undo.AddComponent<T>(target.gameObject);
        }
    }
}

#endif