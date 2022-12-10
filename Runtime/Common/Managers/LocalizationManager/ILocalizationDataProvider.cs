using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Saro.Localization
{
    /// <summary>
    /// 多语言数据提供者。可以是 assetbundle/resources/vfs 等等
    /// </summary>
    public interface ILocalizationDataProvider
    {
        void Load(ELanguage language, Dictionary<int, string> map);

        UniTask<bool> LoadAsync(ELanguage language, Dictionary<int, string> map);
    }
}