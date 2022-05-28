using UnityEngine;

namespace Saro.Utility
{
    public static class ModelUtility
    {
        public static void GetRenderableBoundsRecurse(ref Bounds bounds, GameObject go)
        {
            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            if (meshRenderer && meshFilter && meshFilter.sharedMesh)
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = meshRenderer.bounds;
                }
                else
                {
                    bounds.Encapsulate(meshRenderer.bounds);
                }
            }
            SkinnedMeshRenderer skinnedMeshRenderer = go.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
            if (skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh)
            {
                if (bounds.extents == Vector3.zero)
                {
                    bounds = skinnedMeshRenderer.bounds;
                }
                else
                {
                    bounds.Encapsulate(skinnedMeshRenderer.bounds);
                }
            }
            foreach (Transform transform in go.transform)
            {
                GetRenderableBoundsRecurse(ref bounds, transform.gameObject);
            }
        }
    }
}