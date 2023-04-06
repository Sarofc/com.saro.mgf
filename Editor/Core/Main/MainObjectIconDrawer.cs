using UnityEditor;
using UnityEngine;

namespace Saro
{
    [InitializeOnLoad]
    internal static class MainObjectIconDrawer
    {
        static MainObjectIconDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
        }

        static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj is GameObject go)
            {
                if (go.TryGetComponent<Main>(out var main))
                {
                    var objectContent = EditorGUIUtility.ObjectContent(main, typeof(Main));
                    GUI.DrawTexture(new Rect(selectionRect.xMax - IconSize, selectionRect.yMin, IconSize, IconSize), objectContent.image);
                }
            }
        }


        static readonly int IconSize = 16;
    }
}
