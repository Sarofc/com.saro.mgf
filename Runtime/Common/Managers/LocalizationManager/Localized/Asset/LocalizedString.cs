using Cysharp.Threading.Tasks;
using Saro.Core;

namespace Saro.Localization
{
    public class LocalizedString : ALocalizedAsset<string>
    {
        public LocalizedString(int localizedKey) : base(localizedKey) { }

        public override string GetLocalizedValue() => LocalizationManager.Current.GetLocalizedValue(LocalizedKey);

        public override async UniTask<string> GetLocalizedAssestAsync(IAssetLoader loader)
        {
            await UniTask.CompletedTask;
            return GetLocalizedValue(); // string 特殊
        }
    }
}
