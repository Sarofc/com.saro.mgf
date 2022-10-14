using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Saro.UI
{
    /*
     * issue: 
     * 
     * 加了scrollbar后，滑动变卡，去掉后，就丝滑了
     * 
     */
    public abstract class BaseSuperScrollRect : ScrollRect
    {
        public ISuperScrollRectDataProvider DataProvider { get; private set; }

        public bool isGrid;

        public RectTransform cellPrefab;

        public Vector2 padding;

        public Vector2 spacing;

        public EScrollDir direction;

        public int segment = 1;

        public enum EScrollDir
        {
            Vertical,
            Horizontal
        }

        public List<ScrollRectCell> CellList => m_SuperScrollRectImpl.CellList;

        private SuperScrollRectImpl m_SuperScrollRectImpl;

        public void DoAwake(ISuperScrollRectDataProvider dataProvider)
        {
            DataProvider = dataProvider;
            ReloadData();
        }

        public override void SetLayoutHorizontal()
        {
            base.SetLayoutHorizontal();
        }

        [ContextMenu("ReloadData")]
        public void ReloadData()
        {
            if (DataProvider == null)
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Have no dataProvider, please call DoAwake first");
                }
                return;
            }
            StopMovement();
            vertical = (direction == EScrollDir.Vertical);
            horizontal = (direction == EScrollDir.Horizontal);
            if (m_SuperScrollRectImpl == null)
            {
                m_SuperScrollRectImpl = new SuperScrollRectImpl();
            }
            m_SuperScrollRectImpl.DoAwake(this);
            onValueChanged.RemoveListener(OnValueChanged);
            m_SuperScrollRectImpl.DoStart();
            onValueChanged.AddListener(OnValueChanged);
        }

        public void ClearCache()
        {
            m_SuperScrollRectImpl.ClearCache();
        }

        public void JumpTo(int cellIndex)
        {
            StopMovement();
            m_SuperScrollRectImpl.JumpTo(cellIndex);
        }

        public void SetRefreshSpeed(int maxUpdateCountPerFrame = 6)
        {
            m_SuperScrollRectImpl.MaxUpdateCountPerFrame = maxUpdateCountPerFrame;
        }

        private void OnValueChanged(Vector2 normalizedPos)
        {
            m_SuperScrollRectImpl.Refresh();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            m_SuperScrollRectImpl?.ProcessTasks();
        }
    }

#if UNITY_EDITOR


    [CustomEditor(typeof(BaseSuperScrollRect), true)]
    [CanEditMultipleObjects]
    public class EditorSuperScrollRect : Editor
    {
        public override void OnInspectorGUI()
        {
            BaseSuperScrollRect baseSuperScrollRect = target as BaseSuperScrollRect;
            serializedObject.Update();
            PropertyField("m_Viewport");
            PropertyField("m_Content");
            PropertyField("cellPrefab");
            GUILayout.Space(5f);
            PropertyField("direction");
            PropertyField("isGrid");
            bool isGrid = baseSuperScrollRect.isGrid;
            if (isGrid)
            {
                PropertyField("segment");
            }
            EditorGUI.BeginChangeCheck();
            PropertyField("padding");
            PropertyField("spacing");
            bool flag = EditorGUI.EndChangeCheck();
            if (flag)
            {
                baseSuperScrollRect.ReloadData();
            }
            DrawInternalProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInternalProperties()
        {
            EditorGUILayout.Space();
            PropertyField("m_MovementType");
            PropertyField("m_Elasticity");
            PropertyField("m_Inertia");
            PropertyField("m_DecelerationRate");
            PropertyField("m_ScrollSensitivity");

            EditorGUILayout.Space();
            var verticalScrollbar = PropertyField("m_VerticalScrollbar");
            if (verticalScrollbar.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                PropertyField("m_VerticalScrollbarVisibility");
                PropertyField("m_VerticalScrollbarSpacing");
                EditorGUI.indentLevel--;
            }

            var horizontalScrollbar = PropertyField("m_HorizontalScrollbar");
            if (horizontalScrollbar.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                PropertyField("m_HorizontalScrollbarVisibility");
                PropertyField("m_HorizontalScrollbarSpacing");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            PropertyField("m_OnValueChanged");
        }

        private SerializedProperty PropertyField(string name)
        {
            SerializedProperty serializedProperty = serializedObject.FindProperty(name);
            EditorGUILayout.PropertyField(serializedProperty, Array.Empty<GUILayoutOption>());
            return serializedProperty;
        }
    }

#endif
}
