using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
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

    private HashSet<string> m_BlackList = new()
    {
        "SceneView RT",
        "GUIViewHDRRT",
        "Font Texture",
        "CurveTexture",
        "GizmoIconAtlas_pix32",
    };

    private List<Texture> m_Textures;

    private string m_Search = "";
    private Vector2 m_Scroll;

    private void OnGUI()
    {
        if (GUILayout.Button("Reload"))
        {
            FindTextures(true);
        }

        FindTextures();

        GUILayout.BeginHorizontal("HelpBox");
        GUILayout.Space(30);
        m_Search = EditorGUILayout.TextField("", m_Search, "SearchTextField");
        GUILayout.Label("", "SearchCancelButtonEmpty");
        GUILayout.EndHorizontal();

        m_Scroll = GUILayout.BeginScrollView(m_Scroll);

        foreach (Texture texture in m_Textures)
        {
            if (texture == null) continue;
            var lowerSearch = m_Search.ToLower();
            if (lowerSearch != "" && !texture.name.ToLower().Contains(lowerSearch))
                continue;

            DrawIconItem(texture);
        }
        GUILayout.EndScrollView();
    }

    private void OnDestroy()
    {
        m_Textures.Clear();
    }

    private void FindTextures(bool force = false)
    {
        if (m_Textures == null || force)
        {
            m_Textures = Resources.FindObjectsOfTypeAll<Texture>()
                .Where(texture =>
                {
                    var assetPath = AssetDatabase.GetAssetPath(texture);
                    return !(assetPath.StartsWith("Assets") || assetPath.StartsWith("Packages"));
                })
                .Where(texture =>
                {
                    if (texture == null) return false;

                    if (texture is Cubemap) return false;
                    if (texture is Texture3D) return false;
                    if (texture is Texture2DArray) return false;
                    if (texture is CubemapArray) return false;

                    if (texture.name == "")
                        return false;

                    if (texture.name.Contains("(Generated)")) return false;
                    if (m_BlackList.Contains(texture.name)) return false;

                    return true;
                })
                .ToList();

            m_Textures.Sort((pA, pB) => string.Compare(pA.name, pB.name, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void DrawIconItem(Texture texture)
    {
        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button(texture, GUILayout.Height(60), GUILayout.Width(60)))
        {
            CopyText("EditorGUIUtility.FindTexture(\"" + texture.name + "\")");
        }
        GUILayout.Space(40);
        if (GUILayout.Button(new GUIContent(texture.name, "Save to png"), EditorStyles.linkLabel))
        {
            if (texture is Texture2D _tex)
            {
                var path = EditorUtility.SaveFilePanel("save to png", Application.dataPath, _tex.name, "png");
                if (!string.IsNullOrEmpty(path))
                {
                    // _tex.isReadable == false 时，有的能导出，有的会抛异常 Read/Write，例如 VisualScript 相关的都会异常
                    //Debug.LogError($"{_tex.name}.{nameof(Texture2D.isReadable)} : " + _tex.isReadable);

                    var rawData = _tex.GetRawTextureData(); // must get a copy

                    var tmpTex = new Texture2D(_tex.width, _tex.height, _tex.format, false);
                    tmpTex.LoadRawTextureData(rawData);
                    File.WriteAllBytes(path, tmpTex.EncodeToPNG());
                    GameObject.DestroyImmediate(tmpTex);
                }
            }
            else
            {
                this.ShowNotification(new GUIContent("error. not texture 2d"), 1f);
            }
        }
        //EditorGUILayout.SelectableLabel(texture.name);
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
