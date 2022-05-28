using UnityEngine;

namespace Saro.Diagnostics
{
    public sealed class BoxColliderVisual : MonoBehaviour
    {
        public Color color = Color.red;
        private BoxCollider m_BoxCollider;

        private void Awake()
        {
            m_BoxCollider = GetComponent<BoxCollider>();
        }

        private void Update()
        {
            if (!m_BoxCollider || !m_BoxCollider.enabled) return;

            Vector3 v3Center = m_BoxCollider.center + transform.position;
            Vector3 v3Extents = m_BoxCollider.size / 2f;

            GLDebugHelper.DebugBox(v3Center, v3Extents, color, transform.rotation, Time.deltaTime, preview: EGLDebug.Both);
        }
    }
}