using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace Saro.Localization
{
    [MovedFrom(true, sourceClassName: "TextLocalized")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public class LocalizedTextEvent : ALocalizedEvent<Text>
    {
        protected override void OnValueChanged()
        {
            m_Target.text = m_Localization.GetLocalizedValue(m_LocalizedKey);
        }
    }
}