using System;

namespace Saro.UI
{
    public interface IAnimation
    {
        void SetComponent(IComponent component);
        IAnimation OnStart(Action callback);
        IAnimation OnEnd(Action callback);
        void Play();
    }
}
