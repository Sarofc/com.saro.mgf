#if UNITY_EDITOR && PACKAGE_TMP

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saro.Localization
{
    internal partial class LocalizationComponent
    {
        [MenuItem("CONTEXT/TextMeshProUGUI/Localize")]
        static void LocalizeTMProText(MenuCommand command)
        {
            var target = command.context as UIBehaviour;
            SetupForLocalization<LocalizedTMPTextEvent>(target);
        }

        private static void SetupForLocalization<T>(UIBehaviour target) where T : MonoBehaviour
        {
            Undo.AddComponent<T>(target.gameObject);
        }
    }
}

#endif
