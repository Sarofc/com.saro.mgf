using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saro.Localization
{
    public abstract class ALocalizedEvent<T> : MonoBehaviour where T : Component
    {
        [FormerlySerializedAs("m_Key")]
        [SerializeField] protected string m_LocalizedKey;
        protected T m_Target;

        protected LocalizationManager m_Localization;

        private ELanguage m_LastLanguage = ELanguage.None;

        protected Action m_CachedOnValueChanged; // cache OnValueChanged

        protected virtual void Awake()
        {
            if (!TryGetComponent<T>(out m_Target))
            {
                Log.WARN($"[Localization] Localized Component is null: {typeof(T)}.");
            }

            m_CachedOnValueChanged = OnValueChanged;
        }

        protected virtual void OnEnable()
        {
            if (m_Localization == null) m_Localization = LocalizationManager.Current;
            if (m_Localization == null)
            {
                Log.ERROR("[Localization] LocalizationManager doesn't initialized. Initialize Service First.");
                return;
            }

            if (m_Target == null) return;

            if (m_LastLanguage != m_Localization.CurrentLanguage)
            {
                OnValueChanged();
            }

            m_Localization.onLanguageChanged += m_CachedOnValueChanged;
        }

        protected virtual void OnDisable()
        {
            if (m_Localization == null) return;
            if (m_Target == null) return;

            m_Localization.onLanguageChanged -= m_CachedOnValueChanged;

            m_LastLanguage = m_Localization.CurrentLanguage;
        }

        protected abstract void OnValueChanged();
    }
}