using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Saro.Localization
{
    /*
       虚拟资源地址

       AB or Resources
     */
    public interface ILocalizationDataProvider
    {
        void Load(ELanguage language, Dictionary<int, string> map);

        UniTask<bool> LoadAsync(ELanguage language, Dictionary<int, string> map);
    }
}