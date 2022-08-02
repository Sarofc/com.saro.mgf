using UnityEngine;

namespace Saro.Localization
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnityEngine.UI.Image))]
    public class ImageLocalized : ALocalized<UnityEngine.UI.Image>
    {
        protected override void OnValueChanged()
        {
            throw new System.NotImplementedException("TODO 请先实现！");

            string path = m_Localization.GetValue(m_Key);
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            //var request = Core.Services.GameServices.Get().Resolve<XAsset.AssetMgr>().LoadAsset(path, typeof(Sprite));
            if (sprites != null && sprites.Length > 0)
            {
                if (sprites.Length == 1)
                {
                    m_Target.sprite = sprites[0];
                }
                else
                {
                    string spriteName = m_Target.sprite.name;

                    for (int i = 0; i < sprites.Length; i++)
                    {
                        if (spriteName == sprites[i].name)
                        {
                            m_Target.sprite = sprites[i];
                            break;
                        }
                    }
                }
            }
        }
    }
}