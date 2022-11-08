﻿#if URP_ENABLE

using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Saro.UI
{
    [RequireComponent(typeof(Camera))]
    public sealed class UrpUiCamera : MonoBehaviour
    {
        private IEnumerator Start()
        {
            if (Camera.main == null)
                yield return null;

            var uiCamera = GetComponent<Camera>();
            uiCamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
            Camera.main.GetUniversalAdditionalCameraData().cameraStack.Add(uiCamera);
        }
    }
}

#endif