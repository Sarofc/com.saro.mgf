#if UNITY_EDITOR && PACKAGE_UGUI

using System;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saro.Localization
{
    partial class LocalizationComponent
    {
        [MenuItem("CONTEXT/Text/Localize")]
        static void LocalizeUIText(MenuCommand command)
        {
            var target = command.context as UIBehaviour;
            SetupForLocalization<LocalizedTextEvent>(target);
        }

        [MenuItem("CONTEXT/RawImage/Localize")]
        static void LocalizeUIRawImage(MenuCommand command)
        {
            var target = command.context as UIBehaviour;
            SetupForLocalization<LocalizedRawImageEvent>(target);
        }

        [MenuItem("CONTEXT/Image/Localize")]
        static void LocalizeUIImage(MenuCommand command)
        {
            var target = command.context as UIBehaviour;
            SetupForLocalization<LocalizedImageEvent>(target);
        }
    }
}

#endif