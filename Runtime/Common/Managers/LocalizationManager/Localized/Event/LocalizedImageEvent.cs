using Saro.Core;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace Saro.Localization
{
    [MovedFrom(true, sourceClassName: "ImageLocalized")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class LocalizedImageEvent : ALocalizedEvent<Image>
    {
        private DefaultAssetLoader m_Loader;
        protected override async void OnValueChanged()
        {
            string path = m_Localization.GetLocalizedValue(m_LocalizedKey);
            var sprite = await m_Loader.LoadAssetRefAsync<Sprite>(path);
            m_Target.sprite = sprite;
        }

        protected override void Awake()
        {
            base.Awake();

            m_Loader = IAssetLoader.Create<DefaultAssetLoader>(8, true);
        }

        private void OnDestroy()
        {
            IAssetLoader.Release(m_Loader);
        }
    }
}