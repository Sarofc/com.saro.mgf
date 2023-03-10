#if PACKAGE_TMP

using UnityEngine;
using TMPro;

namespace Saro.Localization
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMPTextEvent : ALocalizedEvent<TMP_Text>
    {
        protected override void OnValueChanged()
        {
            m_Target.text = m_Localization.GetLocalizedValue(m_LocalizedKey);
        }
    }
}

#endif