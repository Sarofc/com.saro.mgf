using System;
using Cysharp.Threading.Tasks;
using Saro.Core;
using UnityEngine;

namespace Saro.Localization
{
    public class LocalizedAudioClip : ALocalizedAsset<AudioClip>
    {
        public LocalizedAudioClip(string localizedKey) : base(localizedKey)
        {
        }

        public override string GetLocalizedValue()
        {
            return LocalizationManager.Current.GetLocalizedValue(LocalizedKey);
        }

        public override async UniTask<AudioClip> GetLocalizedAssestAsync(IAssetLoader loader)
        {
            string path = GetLocalizedValue();
            var _loader = loader ?? Main.MainAssetLoader;
            var clip = await _loader.LoadAssetRefAsync<AudioClip>(path);
            return clip;
        }
    }
}