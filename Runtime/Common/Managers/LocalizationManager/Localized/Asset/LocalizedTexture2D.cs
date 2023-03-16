using System;
using Cysharp.Threading.Tasks;
using Saro.Core;
using UnityEngine;

namespace Saro.Localization
{
    public class LocalizedTexture2D : ALocalizedAsset<Texture2D>
    {
        public LocalizedTexture2D(string localizedKey) : base(localizedKey)
        {
        }

        public override string GetLocalizedValue()
        {
            return LocalizationManager.Current.GetLocalizedValue(LocalizedKey);
        }

        public override async UniTask<Texture2D> GetLocalizedAssestAsync(IAssetLoader loader)
        {
            string path = GetLocalizedValue();
            var _loader = loader ?? Main.MainAssetLoader;
            var clip = await _loader.LoadAssetRefAsync<Texture2D>(path);
            return clip;
        }
    }
}