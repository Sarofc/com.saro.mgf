using Saro.Attributes;
using System.Collections;
using UnityEngine;

namespace Saro.UI
{
    public sealed class UIEffect : MonoBehaviour
    {
        public enum ECreateType
        {
            OnEnable = 0,//UI创建时
            Manual = 1,//手动调用
        }

        public ECreateType createType = ECreateType.OnEnable;

        [SerializeField, AssetPath(typeof(GameObject))]
        private string m_EffectID;//特效ID
        [SerializeField]
        private Vector3 m_EffectScale = Vector3.one;//特效缩放
        [SerializeField]
        private float m_Delay = 0f;

        private GameObject m_EffectInstance;

        private Coroutine m_CreateEffectCoroutine;

        /// <summary>
        /// 执行初始化，只有一次
        /// </summary>
        private void OnEnable()
        {
            if (createType == ECreateType.OnEnable)
            {
                CreateEffect();
            }
        }

        private void OnDisable()
        {
            if (m_CreateEffectCoroutine != null)
            {
                StopCoroutine(m_CreateEffectCoroutine);
            }

            DestroyEffect();
        }

        public void CreateEffect()
        {
            if (m_Delay > 0.03f)
            {
                m_CreateEffectCoroutine = StartCoroutine(_CreateEffect(m_Delay));
            }
            else
            {
                CreateEffectInternal();
            }
        }

        private IEnumerator _CreateEffect(float delay)
        {
            yield return new WaitForSeconds(delay);
            CreateEffectInternal();
        }

        private void CreateEffectInternal()
        {
            // TODO 换成特效 资源管理器
            //var prefab = UIManager.Current.LoadAsset<GameObject>(m_EffectID);
            //m_EffectInstance = GameObject.Instantiate(prefab, m_EffectScale, Quaternion.identity);
        }

        public void DestroyEffect()
        {
            if (m_EffectInstance != null)
            {
                GameObject.Destroy(m_EffectInstance);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("测试特效")]
        private void TestEffect()
        {
            CreateEffectInternal();
        }
#endif
    }

}