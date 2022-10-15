using Saro.SEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Saro.XConsole
{
    internal sealed class ResizeButton : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        [ReadOnly]
        public Configs configs;
        public RectTransform parent;
        public RectTransform window;
        public float minHeight = 180, minWidth = 300;
        public float maxHeight = 500, maxWidth = 700;

        private float Height { get => configs.height; set => configs.height = value; }
        private float Width { get => configs.width; set => configs.width = value; }

        private Vector2 m_StartPos;

        private void Start()
        {
            maxHeight = parent.rect.height;
            maxWidth = parent.rect.width;

            Clamp();

            Resize(Height, Width);
        }

        private void Clamp()
        {
            Height = Mathf.Clamp(Height, minHeight, maxHeight);

            Width = Mathf.Clamp(Width, minWidth, maxWidth);
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
                Height = window.rect.height - delta.y;
                Width = window.rect.width - delta.x;
                m_StartPos = pos;
                Clamp();

                Resize(Height, Width);
            }
        }

        private void Resize(float newHeight, float newWidth)
        {
            window.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            window.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
        }
    }
}
