using Saro.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Saro.Localization
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RawImage))]
    public class LocalizedRawImageEvent : ALocalizedEvent<RawImage>
    {
        private DefaultAssetLoader m_Loader;
        protected override async void OnValueChanged()
        {
            string path = m_Localization.GetLocalizedValue(m_LocalizedKey);
            var texture = await m_Loader.LoadAssetRefAsync<Texture>(path);
            m_Target.texture = texture;
        }

        protected override void Awake()
        {
            base.Awake();

            m_Loader = AssetLoaderFactory.Create<DefaultAssetLoader>(8, true);
        }

        private void OnDestroy()
        {
            AssetLoaderFactory.Release(m_Loader);
        }
    }
}