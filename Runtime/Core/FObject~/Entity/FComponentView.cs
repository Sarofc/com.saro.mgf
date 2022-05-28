//#if UNITY_EDITOR

//using UnityEditor;
//using UnityEngine;

//namespace Saro
//{
//    public class FComponentView : MonoBehaviour
//    {
//#if UNITY_2019_4_OR_NEWER
//        [SerializeReference]
//#endif
//        public FObject Object;

//        [CustomEditor(typeof(FComponentView), true)]
//        internal class FComponentViewInspectoFr : Editor
//        {
//            //public override void OnInspectorGUI()
//            //{
//            //    EditorGUI.BeginChangeCheck();
//            //    serializedObject.UpdateIfRequiredOrScript();
//            //    SerializedProperty iterator = serializedObject.GetIterator();
//            //    bool enterChildren = true;
//            //    while (iterator.NextVisible(enterChildren))
//            //    {
//            //        using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
//            //        {
//            //            EditorGUILayout.PropertyField(iterator, true);
//            //        }

//            //        enterChildren = false;
//            //    }

//            //    if (EditorGUI.EndChangeCheck())
//            //    {
//            //        serializedObject.ApplyModifiedProperties();
//            //    }
//            //}
//        }
//    }
//}

//#endif