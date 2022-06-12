using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public class ScriptGenTemplate
{
    // TODO 搞个 GenericMenu 直接数据驱动

    private const string k_EventScript_Path = @"Packages\com.saro.mgf\Editor\EditorTools\CodeGen\CodeGen.Template\Templates\Template_EventScript.txt";
    private const string k_UIScript_Path = @"Packages\com.saro.mgf\Editor\EditorTools\CodeGen\CodeGen.Template\Templates\Template_UIScript.txt";

    [MenuItem("Assets/Create/Scripts/Event Script", false, 81)]
    private static void CreateNewEventScript()
    {
        CreateNewScript(k_EventScript_Path);
    }

    [MenuItem("Assets/Create/Scripts/UI Script", false, 81)]
    private static void CreateNewUIScript()
    {
        CreateNewScript(k_UIScript_Path);
    }

    private static void CreateNewScript(string templatePath)
    {
        var filePath = GetSelectedPathOrFallback() + "/" + Path.GetFileNameWithoutExtension(templatePath) + ".cs";
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<MyDoCreateScriptAsset>(),
             filePath,
            null,
            templatePath
           );
    }

    public static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }
}

internal class MyDoCreateScriptAsset : EndNameEditAction
{
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
        ProjectWindowUtil.ShowCreatedAsset(o);
    }

    internal static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string templateFile)
    {
        string fullPath = Path.GetFullPath(pathName);

        var sr = new StreamReader(templateFile);
        string text = sr.ReadToEnd();
        sr.Close();

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);

        text = Regex.Replace(text, "#NAMESPACE#", string.IsNullOrEmpty(EditorSettings.projectGenerationRootNamespace) ? "Unnamed" : EditorSettings.projectGenerationRootNamespace);
        text = Regex.Replace(text, "#SCRIPTNAME#", fileNameWithoutExtension);

        bool encoderShouldEmitUTF8Identifier = true;
        bool throwOnInvalidBytes = false;
        UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
        bool append = false;
        var sw = new StreamWriter(fullPath, append, encoding);
        sw.Write(text);
        sw.Close();

        AssetDatabase.ImportAsset(pathName);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
    }
}