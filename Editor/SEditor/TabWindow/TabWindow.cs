using UnityEngine;

namespace Saro.SEditor
{

    public abstract class TabWindow
    {
        public abstract string TabName { get; }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnGUI(Rect rect) { }
    }
}