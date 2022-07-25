using UnityEngine;

namespace Saro.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UICustomOrder : MonoBehaviour
    {
        [Range(1, 100)]
        public int addLayer = 1;

        private Canvas m_ParentCanvas;
        private Canvas m_Canvas;

        /// <summary>
        /// 设置parent canvas
        /// </summary>
        /// <param name="uiCanvas"></param>
        public virtual void SetParentCanvas(Canvas uiCanvas)
        {
            m_ParentCanvas = uiCanvas;
        }

        /// <summary>
        /// 更新order
        /// </summary>
        public virtual void UpdateOrder()
        {
            if (m_Canvas != null)
            {
                m_Canvas.sortingLayerID = m_ParentCanvas.sortingLayerID;
                m_Canvas.sortingOrder = m_ParentCanvas.sortingOrder + addLayer % 100;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_Canvas == null)
            {
                m_Canvas = GetComponent<Canvas>();
            }
        }
#endif
    }
}