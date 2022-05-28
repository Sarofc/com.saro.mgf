using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Saro.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIBinder))]
    public sealed class WindowAnimation_Animator : UIAnimation, IShowWindowAnimation, IHideWindowAnimation
    {
        public float openDuration = 0.2f;
        public string openAnimationName = "UIOpen";

        public float closeDuration = 0.2f;
        public string closeAnimationName = "UIClose";

        [SerializeField]
        private Animator m_Animator;
        private CancellationTokenSource cts;

        private void OnDisable()
        {
            if (cts != null)
                cts.Cancel();
        }

        public override async void Play()
        {
            OnStart();

            m_Animator.Play(!m_Component.IsOpen ? openAnimationName : closeAnimationName);

            try
            {
                if (cts == null || cts.IsCancellationRequested)
                    cts = new CancellationTokenSource();
                await UniTask.Delay((int)(closeDuration * 1000), cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            { }

            OnEnd();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_Animator == null)
                m_Animator = GetComponent<Animator>();
        }
#endif
    }
}