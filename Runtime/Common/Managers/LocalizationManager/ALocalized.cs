using UnityEngine;

namespace Saro.Localization
{
    public abstract class ALocalized<T> : MonoBehaviour where T : Component
    {
        [SerializeField] protected int m_Key;
        protected T m_Target;

        protected LocalizationManager m_Localization;

        private ELanguage m_LastLanguage = ELanguage.None;

        protected virtual void Awake()
        {
            if (m_Target == null) m_Target = GetComponent<T>();
            if (m_Target == null)
            {
                Debug.LogWarning($"Localized Component is null: {typeof(T)}.");
                return;
            }
        }

        protected virtual void OnEnable()
        {
            if (m_Localization == null) m_Localization = LocalizationManager.Current;
            if (m_Localization == null)
            {
                Debug.LogWarning("Localization Service hasn't initialized. Initialize Service First.");
                return;
            }

            m_Localization.onLanguageChanged += OnValueChanged;

            if (m_LastLanguage != m_Localization.CurrentLanguage)
            {
                OnValueChanged();
            }
        }

        protected virtual void OnDisable()
        {
            if (m_Localization == null) return;

            m_Localization.onLanguageChanged -= OnValueChanged;

            m_LastLanguage = m_Localization.CurrentLanguage;
        }

        protected abstract void OnValueChanged();
    }
}