using System;
using UnityEngine;

namespace Saro.UI
{
    public abstract class UIAnimation : MonoBehaviour, IAnimation
    {
        private Action m_OnStart;
        private Action m_OnEnd;
        protected IComponent m_Component;

        protected void OnStart()
        {
            try
            {
                if (this.m_OnStart != null)
                {
                    this.m_OnStart();
                    this.m_OnStart = null;
                }
            }
            catch (Exception) { }
        }

        protected void OnEnd()
        {
            try
            {
                if (this.m_OnEnd != null)
                {
                    this.m_OnEnd();
                    this.m_OnEnd = null;
                }
            }
            catch (Exception) { }
        }

        public IAnimation OnStart(Action onStart)
        {
            this.m_OnStart += onStart;
            return this;
        }

        public IAnimation OnEnd(Action onEnd)
        {
            this.m_OnEnd += onEnd;
            return this;
        }

        public abstract void Play();

        public virtual void SetComponent(IComponent component)
        {
            m_Component = component;
        }
    }
}
