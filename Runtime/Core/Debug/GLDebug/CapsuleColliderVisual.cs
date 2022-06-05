using UnityEngine;

namespace Saro.Diagnostics
{
    public sealed class CapsuleColliderVisual : MonoBehaviour
    {
        public Color color = Color.red;
        private CapsuleCollider m_CapsuleCollider;

        private void Awake()
        {
            m_CapsuleCollider = GetComponent<CapsuleCollider>();
        }

        private void Update()
        {
            if (!m_CapsuleCollider || !m_CapsuleCollider.enabled)
            {
                return;
            }

            Vector3 center = m_CapsuleCollider.center;
            float radius = m_CapsuleCollider.radius;
            float height = m_CapsuleCollider.height;
            var halfHeight = height / 2f;

            var baseSphere = transform.TransformPoint(center);
            var endSphere = transform.TransformPoint(center);

            baseSphere = baseSphere + transform.up * (halfHeight - radius);
            endSphere = endSphere - transform.up * (halfHeight - radius);

            GLDebug.DebugCapsule(baseSphere, endSphere, color, radius, drawDuration: Time.deltaTime, preview: EGLDebug.Both);
        }
    }
}