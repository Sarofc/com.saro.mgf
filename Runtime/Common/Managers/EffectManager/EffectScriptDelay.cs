using UnityEngine;

namespace Saro.Gameplay.Effect
{
    public class EffectScriptDelay : VfxScriptBase
    {
        public float delayToReturn;

        private float m_DelayTimer;

        public override void Init()
        {
            base.Init();

            m_DelayTimer = delayToReturn + Time.time;
        }

        public override void Clean()
        {
            base.Clean();

            m_DelayTimer = 0f;
        }

        protected virtual void Update()
        {
            if (m_DelayTimer <= Time.time)
            {
                Main.Resolve<EffectManager>().ReleaseEffect(this);
            }
        }
    }
}
