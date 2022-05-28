using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Saro.Localization
{
    public sealed class LocalizationManager : IService
    {
        public static LocalizationManager Current => Main.Resolve<LocalizationManager>();

        private ELanguage m_CurrentLanguage = ELanguage.None;

        private ILocalizationDataProvider m_Provider;

        private Dictionary<int, string> m_LanguageLut = new Dictionary<int, string>();

        public FDelegates onLanguageChanged = new(128);

        public ELanguage CurrentLanguage
        {
            get => m_CurrentLanguage;
        }

        public LocalizationManager SetLanguage(ELanguage language)
        {
            if (language == ELanguage.None) return this;
            if (m_CurrentLanguage == language) return this;

            m_CurrentLanguage = language;

            OnLanguageChanged();

            return this;
        }

        public async UniTask SetLanguageAsync(ELanguage language)
        {
            if (language == ELanguage.None) return;
            if (m_CurrentLanguage == language) return;

            m_CurrentLanguage = language;

            await OnLanguageChangedAsync();
        }

        public LocalizationManager SetProvider(ILocalizationDataProvider provider)
        {
            m_Provider = provider;
            return this;
        }

        public string GetValue(int key)
        {
            //if (string.IsNullOrEmpty(key)) UnityEngine.Debug.LogError("key is null");
            if (m_LanguageLut == null) UnityEngine.Debug.LogError("lut is null");
            if (m_LanguageLut.TryGetValue(key, out string value))
            {
                return value;
            }
            return null;
        }

        private void OnLanguageChanged()
        {
            m_LanguageLut.Clear();

            m_Provider.Load(CurrentLanguage, m_LanguageLut);

            onLanguageChanged?.Invoke();
        }

        private async UniTask OnLanguageChangedAsync()
        {
            m_LanguageLut.Clear();

            var result = await m_Provider.LoadAsync(CurrentLanguage, m_LanguageLut);

            if (result)
                onLanguageChanged?.Invoke();
            else
                Log.ERROR($"{m_Provider} LoadAsync error");
        }

        void IService.Awake() { }

        void IService.Update() { }

        void IService.Dispose() { }
    }
}