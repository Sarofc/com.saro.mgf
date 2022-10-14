using UnityEngine;
using UnityEngine.EventSystems;

namespace Saro
{
    internal sealed class ResizeWindow : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        public RectTransform parent;
        public RectTransform window;
        public float minHeight = 180, minWidth = 300;
        public float maxHeight = 500, maxWidth = 700;

        private float m_Height;
        private float m_Width;
        private Vector2 m_StartPos;

        private void Start()
        {
            maxHeight = parent.rect.height;
            maxWidth = parent.rect.width;

            Clamp();
        }

        private void Clamp()
        {
            m_Height = Mathf.Clamp(m_Height, minHeight, maxHeight);

            m_Width = Mathf.Clamp(m_Width, minWidth, maxWidth);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, eventData.position, eventData.pressEventCamera, out var pos))
            {
                m_StartPos = pos;
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, eventData.position, eventData.pressEventCamera, out var pos))
            {
                var delta = pos - m_StartPos;
                m_Height = window.rect.height - delta.y;
                m_Width = window.rect.width - delta.x;
                m_StartPos = pos;
                Clamp();

                Resize(m_Height, m_Width);
            }
        }

        private void Resize(float newHeight, float newWidth)
        {
            window.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            window.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        }
    }
}
