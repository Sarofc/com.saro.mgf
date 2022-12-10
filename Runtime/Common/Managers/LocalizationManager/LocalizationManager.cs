using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Saro.Localization
{
    /*
     * TODO
     * 
     * 1. 到底用 int 作为key，还是 string？
     *    string更可读，但费脑子需要命名。
     *    int直接，直接递增即可，但意义不明，维护成本可能偏高？但似乎可以生成代码(enum)来解决这个问题
     * 
     */
    public sealed class LocalizationManager : IService
    {
        public static LocalizationManager Current => Main.Resolve<LocalizationManager>();
        public ELanguage CurrentLanguage { get; private set; } = ELanguage.None;
        public FDelegates onLanguageChanged = new(128);

        private ILocalizationDataProvider m_Provider;
        private Dictionary<int, string> m_LanguageLut = new();

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
        /// 根据key获取多语言 文本/资源地址
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(int key)
        {
            if (m_LanguageLut == null) Log.ERROR("[Localization] lut is null");
            if (m_LanguageLut.TryGetValue(key, out string value))
                return value;
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
                Log.ERROR($"[Localization] {m_Provider.GetType().FullName} LoadAsync error");
        }

        void IService.Awake() { }
        void IService.Update() { }
        void IService.Dispose() { }
    }
}