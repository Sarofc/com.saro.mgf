using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;

namespace Saro.SaroEditor
{
    public class BaseEditor<T> : Editor where T : class
    {
        private readonly List<string> m_Excluded = new List<string>();

        protected T Target => target as T;

        protected virtual void BeginInspector()
        {
            serializedObject.Update();
            m_Excluded.Clear();
            GetExcludedPropertiesInInspector(m_Excluded);
        }

        /// <summary>
        /// 添加不绘制的字段
        /// </summary>
        /// <param name="excluded"></param>
        protected virtual void GetExcludedPropertiesInInspector(List<string> excluded)
        {
            ExcludeProperty("m_Script");
        }

        public override void OnInspectorGUI()
        {
            BeginInspector();
            DrawRemainingPropertiesInInspector();
        }

        protected SerializedProperty FindAndExcluedeProperty<TValue>(Expression<Func<TValue>> expr)
        {
            SerializedProperty p = serializedObject.FindProperty(expr);
            ExcludeProperty(p.name);
            return p;
        }

        protected void ExcludeProperty(string propertyName)
        {
            m_Excluded.Add(propertyName);
        }

        protected void DrawRemainingPropertiesInInspector()
        {
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, m_Excluded.ToArray());
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}