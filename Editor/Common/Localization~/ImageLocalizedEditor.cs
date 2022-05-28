using UnityEngine;
using UnityEditor;

namespace Saro.Localization
{
    [CustomEditor(typeof(ImageLocalized))]
    public class ImageLocalizedEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("若图集有超过1个元素\n则需要预先设置好图片\n且图集的元素的文件名要一一对应", MessageType.Warning);
        }
    }
}