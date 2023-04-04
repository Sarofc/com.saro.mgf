#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Saro.SEditor
{
    using BoneShape = BoneRenderer.BoneShape;

    /// <summary>
    /// The BoneRenderer component is responsible for displaying pickable bones in the Scene View.
    /// This component does nothing during runtime.
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.1/manual/RiggingWorkflow.html#bone-renderer-component")]
    public class BoneRenderer : MonoBehaviour
    {
        /// <summary>
        /// Shape used by individual bones.
        /// </summary>
        public enum BoneShape
        {
            /// <summary>Bones are rendered with single lines.</summary>
            Line,

            /// <summary>Bones are rendered with pyramid shapes.</summary>
            Pyramid,

            /// <summary>Bones are rendered with box shapes.</summary>
            Box
        };

        /// <summary>Shape of the bones.</summary>
        public BoneShape boneShape = BoneShape.Pyramid;

        /// <summary>Toggles whether to render bone shapes or not.</summary>
        public bool drawBones = true;

        /// <summary>Toggles whether to draw tripods on bones or not.</summary>
        public bool drawTripods = false;

        /// <summary>Size of the bones.</summary>
        [Range(0.01f, 5.0f)] public float boneSize = 1.0f;

        /// <summary>Size of the tripod axis.</summary>
        [Range(0.01f, 5.0f)] public float tripodSize = 1.0f;

        /// <summary>Color of the bones.</summary>
        public Color boneColor = new Color(0f, 0f, 1f, 0.5f);

        [SerializeField] private Transform[] m_Transforms;

        /// <summary>Transform references in the BoneRenderer hierarchy that are used to build bones.</summary>
        public Transform[] transforms
        {
            get { return m_Transforms; }
#if UNITY_EDITOR
            set
            {
                m_Transforms = value;
                ExtractBones();
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Bone described by two Transform references.
        /// </summary>
        public struct TransformPair
        {
            public Transform first;
            public Transform second;
        };

        private TransformPair[] m_Bones;
        private Transform[] m_Tips;

        /// <summary>Retrieves the bones isolated from the Transform references.</summary>
        /// <seealso cref="BoneRenderer.transforms"/>
        public TransformPair[] bones
        {
            get => m_Bones;
        }

        /// <summary>Retrieves the tip bones isolated from the Transform references.</summary>
        /// <seealso cref="BoneRenderer.transforms"/>
        public Transform[] tips
        {
            get => m_Tips;
        }

        /// <summary>
        /// Delegate function that covers a BoneRenderer calling OnEnable.
        /// </summary>
        /// <param name="boneRenderer">The BoneRenderer component</param>
        public delegate void OnAddBoneRendererCallback(BoneRenderer boneRenderer);

        /// <summary>
        /// Delegate function that covers a BoneRenderer calling OnDisable.
        /// </summary>
        /// <param name="boneRenderer">The BoneRenderer component</param>
        public delegate void OnRemoveBoneRendererCallback(BoneRenderer boneRenderer);

        /// <summary>
        /// Notification callback that is sent whenever a BoneRenderer calls OnEnable.
        /// </summary>
        public static OnAddBoneRendererCallback onAddBoneRenderer;

        /// <summary>
        /// Notification callback that is sent whenever a BoneRenderer calls OnDisable.
        /// </summary>
        public static OnRemoveBoneRendererCallback onRemoveBoneRenderer;

        void OnEnable()
        {
            ExtractBones();
            onAddBoneRenderer?.Invoke(this);
        }

        void OnDisable()
        {
            onRemoveBoneRenderer?.Invoke(this);
        }

        /// <summary>
        /// Invalidate and Rebuild bones and tip bones from Transform references.
        /// </summary>
        public void Invalidate()
        {
            ExtractBones();
        }

        /// <summary>
        /// Resets the BoneRenderer to default values.
        /// </summary>
        public void Reset()
        {
            ClearBones();
        }

        /// <summary>
        /// Clears bones and tip bones.
        /// </summary>
        public void ClearBones()
        {
            m_Bones = null;
            m_Tips = null;
        }

        /// <summary>
        /// Builds bones and tip bones from Transform references.
        /// </summary>
        public void ExtractBones()
        {
            if (m_Transforms == null || m_Transforms.Length == 0)
            {
                ClearBones();
                return;
            }

            var transformsHashSet = new HashSet<Transform>(m_Transforms);

            var bonesList = new List<TransformPair>(m_Transforms.Length);
            var tipsList = new List<Transform>(m_Transforms.Length);

            for (int i = 0; i < m_Transforms.Length; ++i)
            {
                bool hasValidChildren = false;

                var transform = m_Transforms[i];
                if (transform == null)
                    continue;

                if (UnityEditor.SceneVisibilityManager.instance.IsHidden(transform.gameObject, false))
                    continue;

                var mask = UnityEditor.Tools.visibleLayers;
                if ((mask & (1 << transform.gameObject.layer)) == 0)
                    continue;

                if (transform.childCount > 0)
                {
                    for (var k = 0; k < transform.childCount; ++k)
                    {
                        var childTransform = transform.GetChild(k);

                        if (transformsHashSet.Contains(childTransform))
                        {
                            bonesList.Add(new TransformPair() { first = transform, second = childTransform });
                            hasValidChildren = true;
                        }
                    }
                }

                if (!hasValidChildren)
                {
                    tipsList.Add(transform);
                }
            }

            m_Bones = bonesList.ToArray();
            m_Tips = tipsList.ToArray();
        }

        [ContextMenu("Bone Renderer Setup", false, 13)]
        void BoneRendererSetup()
        {
            var selection = this.transform;
            if (selection == null)
                return;

            AnimationRiggingEditorUtils.BoneRendererSetup(selection);
        }
#endif // UNITY_EDITOR
    }

    internal static class AnimationRiggingEditorUtils
    {
        public static void BoneRendererSetup(Transform transform)
        {
            var boneRenderer = transform.GetComponent<BoneRenderer>();
            if (boneRenderer == null)
                boneRenderer = Undo.AddComponent<BoneRenderer>(transform.gameObject);
            else
                Undo.RecordObject(boneRenderer, "Bone renderer setup.");

            var animator = transform.GetComponent<Animator>();
            var renderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            var bones = new List<Transform>();
            if (animator != null && renderers != null && renderers.Length > 0)
            {
                for (int i = 0; i < renderers.Length; ++i)
                {
                    var renderer = renderers[i];
                    for (int j = 0; j < renderer.bones.Length; ++j)
                    {
                        var bone = renderer.bones[j];
                        if (!bones.Contains(bone))
                        {
                            bones.Add(bone);

                            for (int k = 0; k < bone.childCount; k++)
                            {
                                if (!bones.Contains(bone.GetChild(k)))
                                    bones.Add(bone.GetChild(k));
                            }
                        }
                    }
                }
            }
            else
            {
                bones.AddRange(transform.GetComponentsInChildren<Transform>());
            }

            boneRenderer.transforms = bones.ToArray();

            if (PrefabUtility.IsPartOfPrefabInstance(boneRenderer))
                EditorUtility.SetDirty(boneRenderer);
        }

        public static void RestoreBindPose(Transform transform)
        {
            var animator = transform.GetComponentInParent<Animator>();
            var root = (animator) ? animator.transform : transform;
            var renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();

            if (renderers.Length == 0)
            {
                Debug.LogError(
                    string.Format(
                        "Could not restore bind pose because no SkinnedMeshRenderers " +
                        "were found  on {0} or any of its children.", root.name));
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Restore bind pose");

            var bones = new Dictionary<Transform, Matrix4x4>();
            foreach (var renderer in renderers)
            {
                for (int i = 0; i < renderer.bones.Length; ++i)
                {
                    if (!bones.ContainsKey(renderer.bones[i]))
                        bones.Add(renderer.bones[i], renderer.sharedMesh.bindposes[i]);
                }
            }

            var transforms = transform.GetComponentsInChildren<Transform>();
            var restoredPose = false;
            foreach (var t in transforms)
            {
                if (!bones.ContainsKey(t))
                    continue;

                // The root bone is the only bone in the skeleton
                // hierarchy that does not have a parent bone.
                var isRootBone = !bones.ContainsKey(t.parent);

                var matrix = bones[t];
                var wMatrix = matrix.inverse;

                if (!isRootBone)
                {
                    if (t.parent)
                        matrix *= bones[t.parent].inverse;
                    matrix = matrix.inverse;

                    t.localScale = new Vector3(
                        matrix.GetColumn(0).magnitude,
                        matrix.GetColumn(1).magnitude,
                        matrix.GetColumn(2).magnitude
                        );
                    t.localPosition = matrix.MultiplyPoint(Vector3.zero);
                }
                t.rotation = wMatrix.rotation;

                restoredPose = true;
            }

            if (!restoredPose)
            {
                Debug.LogWarning(
                    string.Format(
                        "No valid bindpose(s) have been found for the selected transform: {0}.",
                        transform.name));
            }
        }
    }

    [InitializeOnLoad]
    static class BoneRendererUtils
    {
        private class BatchRenderer
        {
            const int kMaxDrawMeshInstanceCount = 1023;

            public enum SubMeshType
            {
                BoneFaces,
                BoneWire,
                Count
            }

            public Mesh mesh;
            public Material material;

            private List<Matrix4x4> m_Matrices = new List<Matrix4x4>();
            private List<Vector4> m_Colors = new List<Vector4>();
            private List<Vector4> m_Highlights = new List<Vector4>();

            public void AddInstance(Matrix4x4 matrix, Color color, Color highlight)
            {
                m_Matrices.Add(matrix);
                m_Colors.Add(color);
                m_Highlights.Add(highlight);
            }

            public void Clear()
            {
                m_Matrices.Clear();
                m_Colors.Clear();
                m_Highlights.Clear();
            }

            private static int RenderChunkCount(int totalCount)
            {
                return Mathf.CeilToInt((totalCount / (float)kMaxDrawMeshInstanceCount));
            }

            private static T[] GetRenderChunk<T>(List<T> array, int chunkIndex)
            {
                int rangeCount = (chunkIndex < (RenderChunkCount(array.Count) - 1)) ?
                    kMaxDrawMeshInstanceCount : array.Count - (chunkIndex * kMaxDrawMeshInstanceCount);

                return array.GetRange(chunkIndex * kMaxDrawMeshInstanceCount, rangeCount).ToArray();
            }

            public void Render()
            {
                if (m_Matrices.Count == 0 || m_Colors.Count == 0 || m_Highlights.Count == 0)
                    return;

                int count = System.Math.Min(m_Matrices.Count, System.Math.Min(m_Colors.Count, m_Highlights.Count));

                Material mat = material;
                mat.SetPass(0);

                MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                CommandBuffer cb = new CommandBuffer();

                Matrix4x4[] matrices = null;

                int chunkCount = RenderChunkCount(count);
                for (int i = 0; i < chunkCount; ++i)
                {
                    cb.Clear();
                    matrices = GetRenderChunk(m_Matrices, i);
                    propertyBlock.SetVectorArray("_Color", GetRenderChunk(m_Colors, i));

                    material.DisableKeyword("WIRE_ON");
                    cb.DrawMeshInstanced(mesh, (int)SubMeshType.BoneFaces, material, 0, matrices, matrices.Length, propertyBlock);
                    Graphics.ExecuteCommandBuffer(cb);

                    cb.Clear();
                    propertyBlock.SetVectorArray("_Color", GetRenderChunk(m_Highlights, i));

                    material.EnableKeyword("WIRE_ON");
                    cb.DrawMeshInstanced(mesh, (int)SubMeshType.BoneWire, material, 0, matrices, matrices.Length, propertyBlock);
                    Graphics.ExecuteCommandBuffer(cb);
                }
            }
        }

        static List<BoneRenderer> s_BoneRendererComponents = new List<BoneRenderer>();

        private static BatchRenderer s_PyramidMeshRenderer;
        private static BatchRenderer s_BoxMeshRenderer;

        private static Material s_Material;

        private const float k_Epsilon = 1e-5f;

        private const float k_BoneBaseSize = 2f;
        private const float k_BoneTipSize = 0.5f;

        private static int s_ButtonHash = "BoneHandle".GetHashCode();

        private static int s_VisibleLayersCache = 0;

        static BoneRendererUtils()
        {
            BoneRenderer.onAddBoneRenderer += OnAddBoneRenderer;
            BoneRenderer.onRemoveBoneRenderer += OnRemoveBoneRenderer;
            SceneVisibilityManager.visibilityChanged += OnVisibilityChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;

            SceneView.duringSceneGui += DrawSkeletons;

            s_VisibleLayersCache = Tools.visibleLayers;
        }

        private static Material material
        {
            get
            {
                if (!s_Material)
                {
                    Shader shader = (Shader)EditorGUIUtility.LoadRequired("BoneHandles.shader");
                    s_Material = new Material(shader);
                    s_Material.hideFlags = HideFlags.DontSaveInEditor;
                    s_Material.enableInstancing = true;
                }

                return s_Material;
            }
        }

        private static BatchRenderer pyramidMeshRenderer
        {
            get
            {
                if (s_PyramidMeshRenderer == null)
                {
                    var mesh = new Mesh();
                    mesh.name = "BoneRendererPyramidMesh";
                    mesh.subMeshCount = (int)BatchRenderer.SubMeshType.Count;
                    mesh.hideFlags = HideFlags.DontSave;

                    // Bone vertices
                    Vector3[] vertices = new Vector3[]
                    {
                        new Vector3(0.0f, 1.0f, 0.0f),
                        new Vector3(0.0f, 0.0f, -1.0f),
                        new Vector3(-0.9f, 0.0f, 0.5f),
                        new Vector3(0.9f, 0.0f, 0.5f),
                    };

                    mesh.vertices = vertices;

                    // Build indices for different sub meshes
                    int[] boneFaceIndices = new int[]
                    {
                        0, 2, 1,
                        0, 1, 3,
                        0, 3, 2,
                        1, 2, 3
                    };
                    mesh.SetIndices(boneFaceIndices, MeshTopology.Triangles, (int)BatchRenderer.SubMeshType.BoneFaces);

                    int[] boneWireIndices = new int[]
                    {
                        0, 1, 0, 2, 0, 3, 1, 2, 2, 3, 3, 1
                    };
                    mesh.SetIndices(boneWireIndices, MeshTopology.Lines, (int)BatchRenderer.SubMeshType.BoneWire);

                    s_PyramidMeshRenderer = new BatchRenderer()
                    {
                        mesh = mesh,
                        material = material
                    };
                }

                return s_PyramidMeshRenderer;
            }
        }

        private static BatchRenderer boxMeshRenderer
        {
            get
            {
                if (s_BoxMeshRenderer == null)
                {
                    var mesh = new Mesh();
                    mesh.name = "BoneRendererBoxMesh";
                    mesh.subMeshCount = (int)BatchRenderer.SubMeshType.Count;
                    mesh.hideFlags = HideFlags.DontSave;

                    // Bone vertices
                    Vector3[] vertices = new Vector3[]
                    {
                        new Vector3(-0.5f, 0.0f, 0.5f),
                        new Vector3(0.5f, 0.0f, 0.5f),
                        new Vector3(0.5f, 0.0f, -0.5f),
                        new Vector3(-0.5f, 0.0f, -0.5f),
                        new Vector3(-0.5f, 1.0f, 0.5f),
                        new Vector3(0.5f, 1.0f, 0.5f),
                        new Vector3(0.5f, 1.0f, -0.5f),
                        new Vector3(-0.5f, 1.0f, -0.5f)
                    };

                    mesh.vertices = vertices;

                    // Build indices for different sub meshes
                    int[] boneFaceIndices = new int[]
                    {
                        0, 2, 1,
                        0, 3, 2,

                        0, 1, 5,
                        0, 5, 4,

                        1, 2, 6,
                        1, 6, 5,

                        2, 3, 7,
                        2, 7, 6,

                        3, 0, 4,
                        3, 4, 7,

                        4, 5, 6,
                        4, 6, 7
                    };
                    mesh.SetIndices(boneFaceIndices, MeshTopology.Triangles, (int)BatchRenderer.SubMeshType.BoneFaces);

                    int[] boneWireIndices = new int[]
                    {
                        0, 1, 1, 2, 2, 3, 3, 0,
                        4, 5, 5, 6, 6, 7, 7, 4,
                        0, 4, 1, 5, 2, 6, 3, 7
                    };
                    mesh.SetIndices(boneWireIndices, MeshTopology.Lines, (int)BatchRenderer.SubMeshType.BoneWire);

                    s_BoxMeshRenderer = new BatchRenderer()
                    {
                        mesh = mesh,
                        material = material
                    };

                }

                return s_BoxMeshRenderer;
            }
        }

        private static Matrix4x4 ComputeBoneMatrix(Vector3 start, Vector3 end, float length, float size)
        {
            Vector3 direction = (end - start) / length;
            Vector3 tangent = Vector3.Cross(direction, Vector3.up);
            if (Vector3.SqrMagnitude(tangent) < 0.1f)
                tangent = Vector3.Cross(direction, Vector3.right);
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(direction, tangent);

            float scale = length * k_BoneBaseSize * size;

            return new Matrix4x4(
                new Vector4(tangent.x * scale, tangent.y * scale, tangent.z * scale, 0f),
                new Vector4(direction.x * length, direction.y * length, direction.z * length, 0f),
                new Vector4(bitangent.x * scale, bitangent.y * scale, bitangent.z * scale, 0f),
                new Vector4(start.x, start.y, start.z, 1f));
        }

        static void DrawSkeletons(SceneView sceneview)
        {
            if (Tools.visibleLayers != s_VisibleLayersCache)
            {
                OnVisibilityChanged();
                s_VisibleLayersCache = Tools.visibleLayers;
            }

            var gizmoColor = Gizmos.color;

            pyramidMeshRenderer.Clear();
            boxMeshRenderer.Clear();

            for (var i = 0; i < s_BoneRendererComponents.Count; i++)
            {
                var boneRenderer = s_BoneRendererComponents[i];

                if (boneRenderer.bones == null)
                    continue;

                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                {
                    StageHandle stageHandle = prefabStage.stageHandle;
                    if (stageHandle.IsValid() && !stageHandle.Contains(boneRenderer.gameObject))
                        continue;
                }

                if (boneRenderer.drawBones)
                {
                    var size = boneRenderer.boneSize * 0.025f;
                    var shape = boneRenderer.boneShape;
                    var color = boneRenderer.boneColor;
                    var nubColor = new Color(color.r, color.g, color.b, color.a);
                    var selectionColor = Color.white;

                    for (var j = 0; j < boneRenderer.bones.Length; j++)
                    {
                        var bone = boneRenderer.bones[j];
                        if (bone.first == null || bone.second == null)
                            continue;

                        DoBoneRender(bone.first, bone.second, shape, color, size);
                    }

                    for (var k = 0; k < boneRenderer.tips.Length; k++)
                    {
                        var tip = boneRenderer.tips[k];
                        if (tip == null)
                            continue;

                        DoBoneRender(tip, null, shape, color, size);
                    }
                }

                if (boneRenderer.drawTripods)
                {
                    var size = boneRenderer.tripodSize * 0.025f;
                    for (var j = 0; j < boneRenderer.transforms.Length; j++)
                    {
                        var tripodSize = 1f;
                        var transform = boneRenderer.transforms[j];
                        if (transform == null)
                            continue;

                        var position = transform.position;
                        var xAxis = position + transform.rotation * Vector3.right * size * tripodSize;
                        var yAxis = position + transform.rotation * Vector3.up * size * tripodSize;
                        var zAxis = position + transform.rotation * Vector3.forward * size * tripodSize;

                        Handles.color = Color.red;
                        Handles.DrawLine(position, xAxis);
                        Handles.color = Color.green;
                        Handles.DrawLine(position, yAxis);
                        Handles.color = Color.blue;
                        Handles.DrawLine(position, zAxis);
                    }
                }
            }

            pyramidMeshRenderer.Render();
            boxMeshRenderer.Render();

            Gizmos.color = gizmoColor;
        }


        private static void DoBoneRender(Transform transform, Transform childTransform, BoneShape shape, Color color, float size)
        {
            Vector3 start = transform.position;
            Vector3 end = childTransform != null ? childTransform.position : start;

            GameObject boneGO = transform.gameObject;

            float length = (end - start).magnitude;
            bool tipBone = (length < k_Epsilon);

            int id = GUIUtility.GetControlID(s_ButtonHash, FocusType.Passive);
            Event evt = Event.current;

            switch (evt.GetTypeForControl(id))
            {
                case EventType.Layout:
                    {
                        HandleUtility.AddControl(id, tipBone ? HandleUtility.DistanceToCircle(start, k_BoneTipSize * size * 0.5f) : HandleUtility.DistanceToLine(start, end));
                        break;
                    }
                case EventType.MouseMove:
                    if (id == HandleUtility.nearestControl)
                        HandleUtility.Repaint();
                    break;
                case EventType.MouseDown:
                    {
                        if (evt.alt)
                            break;

                        if (HandleUtility.nearestControl == id && evt.button == 0)
                        {
                            if (!SceneVisibilityManager.instance.IsPickingDisabled(boneGO, false))
                            {
                                GUIUtility.hotControl = id; // Grab mouse focus
                                HandleClickSelection(boneGO, evt);
                                evt.Use();
                            }
                        }
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        if (!evt.alt && GUIUtility.hotControl == id)
                        {
                            if (!SceneVisibilityManager.instance.IsPickingDisabled(boneGO, false))
                            {
                                DragAndDrop.PrepareStartDrag();
                                DragAndDrop.objectReferences = new UnityEngine.Object[] { transform };
                                DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(transform));

                                GUIUtility.hotControl = 0;

                                evt.Use();
                            }
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (GUIUtility.hotControl == id && (evt.button == 0 || evt.button == 2))
                        {
                            GUIUtility.hotControl = 0;
                            evt.Use();
                        }
                        break;
                    }
                case EventType.Repaint:
                    {
                        Color highlight = color;

                        bool hoveringBone = GUIUtility.hotControl == 0 && HandleUtility.nearestControl == id;
                        hoveringBone = hoveringBone && !SceneVisibilityManager.instance.IsPickingDisabled(transform.gameObject, false);

                        if (hoveringBone)
                        {
                            highlight = Handles.preselectionColor;
                        }
                        else if (Selection.Contains(boneGO) || Selection.activeObject == boneGO)
                        {
                            highlight = Handles.selectedColor;
                        }

                        if (tipBone)
                        {
                            Handles.color = highlight;
                            Handles.SphereHandleCap(0, start, Quaternion.identity, k_BoneTipSize * size, EventType.Repaint);
                        }
                        else if (shape == BoneShape.Line)
                        {
                            Handles.color = highlight;
                            Handles.DrawLine(start, end);
                        }
                        else
                        {
                            if (shape == BoneShape.Pyramid)
                                pyramidMeshRenderer.AddInstance(ComputeBoneMatrix(start, end, length, size), color, highlight);
                            else // if (shape == BoneShape.Box)
                                boxMeshRenderer.AddInstance(ComputeBoneMatrix(start, end, length, size), color, highlight);
                        }

                    }
                    break;
            }
        }

        public static void OnAddBoneRenderer(BoneRenderer obj)
        {
            s_BoneRendererComponents.Add(obj);
        }

        public static void OnRemoveBoneRenderer(BoneRenderer obj)
        {
            s_BoneRendererComponents.Remove(obj);
        }

        public static void OnVisibilityChanged()
        {
            foreach (var boneRenderer in s_BoneRendererComponents)
            {
                boneRenderer.Invalidate();
            }

            SceneView.RepaintAll();
        }

        public static void OnHierarchyChanged()
        {
            foreach (var boneRenderer in s_BoneRendererComponents)
            {
                boneRenderer.Invalidate();
            }

            SceneView.RepaintAll();
        }

        public static void HandleClickSelection(GameObject gameObject, Event evt)
        {
            if (evt.shift || EditorGUI.actionKey)
            {
                UnityEngine.Object[] existingSelection = Selection.objects;

                // For shift, we check if EXACTLY the active GO is hovered by mouse and then subtract. Otherwise additive.
                // For control/cmd, we check if ANY of the selected GO is hovered by mouse and then subtract. Otherwise additive.
                // Control/cmd takes priority over shift.
                bool subtractFromSelection = EditorGUI.actionKey ? Selection.Contains(gameObject) : Selection.activeGameObject == gameObject;
                if (subtractFromSelection)
                {
                    // subtract from selection
                    var newSelection = new UnityEngine.Object[existingSelection.Length - 1];

                    int index = Array.IndexOf(existingSelection, gameObject);

                    System.Array.Copy(existingSelection, newSelection, index);
                    System.Array.Copy(existingSelection, index + 1, newSelection, index, newSelection.Length - index);

                    Selection.objects = newSelection;
                }
                else
                {
                    // add to selection
                    var newSelection = new UnityEngine.Object[existingSelection.Length + 1];
                    System.Array.Copy(existingSelection, newSelection, existingSelection.Length);
                    newSelection[existingSelection.Length] = gameObject;

                    Selection.objects = newSelection;
                }
            }
            else
                Selection.activeObject = gameObject;
        }
    }
}

#endif
