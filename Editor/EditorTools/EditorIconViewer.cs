using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorIconViewer : EditorWindow
{
    [MenuItem("Window/EidtorIcon查看器")]
    public static void ShowWindow()
    {
        var w = EditorWindow.GetWindow<EditorIconViewer>();
        w.Show();
    }

    private HashSet<string> blackList = new HashSet<string>
    {
       "SceneView RT",
       "GUIViewHDRRT",
       "Font Texture",
       "CurveTexture",
       "GizmoIconAtlas_pix32",
    };

    private List<UnityEngine.Object> _objects;

    private string _search = "";
    private Vector2 scroll;

    private void OnGUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        GUILayout.Space(30);
        _search = EditorGUILayout.TextField("", _search, "SearchTextField", GUILayout.MaxWidth(position.x / 3));
        GUILayout.Label("", "SearchCancelButtonEmpty");
        GUILayout.EndHorizontal();

        if (_objects == null)
        {
            _objects = Resources.FindObjectsOfTypeAll(typeof(Texture)).Where(o =>
            {
                var assetPath = AssetDatabase.GetAssetPath(o);
                return !(assetPath.StartsWith("Assets") || assetPath.StartsWith("Packages"));
            }).ToList();
            _objects.Sort((pA, pB) => string.Compare(pA.name, pB.name, StringComparison.OrdinalIgnoreCase));
        }

        scroll = GUILayout.BeginScrollView(scroll);

        foreach (UnityEngine.Object oo in _objects)
        {
            if (oo is Cubemap) continue;
            if (oo is Texture3D) continue;
            if (oo is Texture2DArray) continue;
            if (oo is CubemapArray) continue;

            Texture texture = oo as Texture;

            if (texture == null) continue;

            //Debug.LogError(AssetDatabase.GetAssetPath(oo));

            if (texture.name == "")
                continue;

            if (blackList.Contains(texture.name)) continue;

            var lowerSearch = _search.ToLower();
            if (lowerSearch != "" && !texture.name.ToLower().Contains(lowerSearch))
                continue;

            DrawIconItem(texture);
        }
        GUILayout.EndScrollView();
    }

    private void DrawIconItem(Texture texture)
    {
        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button(texture, GUILayout.Height(60), GUILayout.Width(60)))
        {
            CopyText("EditorGUIUtility.FindTexture(\"" + texture.name + "\")");
        }
        GUILayout.Space(40);
        EditorGUILayout.SelectableLabel(texture.name);
        GUILayout.Space(50);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void CopyText(string pText)
    {
        TextEditor editor = new TextEditor();

        //editor.content = new GUIContent(pText); // Unity 4.x code
        editor.text = pText; // Unity 5.x code

        editor.SelectAll();
        editor.Copy();
    }
}
