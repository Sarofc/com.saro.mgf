using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Saro.Localization
{
    /*
     * TODO
     * 1. 编辑器模式，可以在面板选择key
     * 2. excel可以分多张表，分别处理 字符串、资源
     */

    public sealed class LocalizationManager : IService
    {
        public static LocalizationManager Current => Main.Resolve<LocalizationManager>();
        public ELanguage CurrentLanguage { get; private set; } = ELanguage.None;
        public FDelegates onLanguageChanged = new(128);

        private ILocalizationDataProvider m_Provider;
        private Dictionary<string, string> m_LanguageLut = new(StringComparer.Ordinal); // 大小写敏感

        public LocalizationManager SetLanguage(ELanguage language)
        {
            if (language == ELanguage.None) return this;
            if (CurrentLanguage == language) return this;
            CurrentLanguage = language;
            OnLanguageChanged();
            return this;
        }

        public async UniTask<LocalizationManager> SetLanguageAsync(ELanguage language)
        {
            if (language == ELanguage.None) return this;
            if (CurrentLanguage == language) return this;
            CurrentLanguage = language;
            await OnLanguageChangedAsync();
            return this;
        }

        public LocalizationManager SetProvider(ILocalizationDataProvider provider)
        {
            m_Provider = provider;
            return this;
        }

        /// <summary>
        /// 根据key获取多语言 文本
        /// </summary>
        /// <param name="localizedKey"></param>
        /// <returns></returns>
        public string GetLocalizedValue(string localizedKey)
        {
            if (m_LanguageLut == null) Log.ERROR("[Localization] lut is null");
            if (m_LanguageLut.TryGetValue(localizedKey, out string value))
                return value;
            return $"!LocalizedKey '{localizedKey}' Not Found";
        }

        [System.Obsolete("Use 'GetLocalizedValue' instead")]
        public string GetValue(string localizedKey)
        {
            return GetLocalizedValue(localizedKey);
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
                Log.ERROR($"[Localization] {m_Provider.GetType().FullName} LoadAsync error");
        }
    }
}