using DG.Tweening;
using UnityEngine;

namespace Saro.UI
{
    /*
     * example UIAnimation
     */

    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIBinder))]
    public class WindowAnimation_Dotween : UIAnimation, IShowWindowAnimation, IHideWindowAnimation
    {
        public Transform AnimatedTransform => root == null ? transform : root;
        public Transform root;

        public float openDuration = 0.2f;
        public Ease oopenEase = Ease.OutBounce;

        public float closeDuration = 0.2f;
        public Ease closeEase = Ease.Linear;

        private IComponent m_Component;

        public override void SetComponent(IComponent component)
        {
            base.SetComponent(component);

            m_Component = component;
        }

        public override void Play()
        {
            OnStart();

            if (m_Component.IsOpen)
            {
                Vector3 endValue = new Vector3(0.0f, 0.0f, 0.0f);

                AnimatedTransform.DOScale(endValue, closeDuration)
                    .SetEase(closeEase)
                    .OnComplete(OnEnd)
                    .SetUpdate(true);
            }
            else
            {
                AnimatedTransform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
                Vector3 endValue = new Vector3(1.0f, 1.0f, 1);

                AnimatedTransform.DOScale(endValue, openDuration)
                    .SetEase(oopenEase)
                    .OnComplete(OnEnd)
                    .SetUpdate(true);
            }
        }
    }
}
