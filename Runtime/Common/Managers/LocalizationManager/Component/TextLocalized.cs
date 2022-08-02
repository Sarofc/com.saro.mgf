using UnityEngine;

namespace Saro.Localization
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class TextLocalized : ALocalized<UnityEngine.UI.Text>
    {
        protected override void OnValueChanged()
        {
            m_Target.text = m_Localization.GetValue(m_Key);
        }
    }
}