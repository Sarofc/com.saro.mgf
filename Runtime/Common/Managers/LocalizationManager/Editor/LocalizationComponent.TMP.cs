#if UNITY_EDITOR && PACKAGE_TMP

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saro.Localization
{
    partial class LocalizationComponent
    {
        [MenuItem("CONTEXT/TextMeshProUGUI/Localize")]
        static void LocalizeTMProText(MenuCommand command)
        {
            var target = command.context as UIBehaviour;
            SetupForLocalization<LocalizedTMPTextEvent>(target);
        }
    }
}

#endif
