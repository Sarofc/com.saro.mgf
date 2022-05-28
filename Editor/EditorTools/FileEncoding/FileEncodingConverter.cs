using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Saro.Utility
{
    public class FileEncodingConverter : EditorWindow
    {
        private const string k_EDITOR_VERSION = "v0.01";
        private string m_FilePath = "";
        private string m_FileSuffixs = ".cs";

        private const string k_FILE_PATH_KEY = "Saro.FileEncodingConverter.m_FilePath";
        private EFileEncoding m_FileEncoding;

        private enum EFileEncoding
        {
            UTF8WithBOM,
            UTF8,
            Unicode,
            BigEndianUnicode,
        }

        [MenuItem("MGF Tools/Utilities/FileEncodingConverter")]
        private static void Init()
        {
            var window = EditorWindow.GetWindow(typeof(FileEncodingConverter));
            window.titleContent = new GUIContent(nameof(FileEncodingConverter));
        }

        private void OnGUI()
        {
            GUILayout.Label("工具版本号: " + k_EDITOR_VERSION);
            GUILayout.Space(5);

            var rect = EditorGUILayout.GetControlRect();
            var fileRect = rect;
            var btnWidth = 50f;
            fileRect.width -= btnWidth;
            var fileBtnRect = rect;
            fileBtnRect.x += fileRect.width;
            fileBtnRect.width = btnWidth;

            var tmpFilePath = EditorPrefs.GetString(k_FILE_PATH_KEY);
            tmpFilePath = EditorGUI.TextField(fileRect, "要转化的路径: ", tmpFilePath);
            if (GUI.Button(fileBtnRect, "Folder"))
            {
                tmpFilePath = EditorUtility.OpenFolderPanel("Select FilePath", tmpFilePath, string.Empty);
            }

            if (tmpFilePath != m_FilePath)
            {
                m_FilePath = tmpFilePath;
                EditorPrefs.SetString(k_FILE_PATH_KEY, m_FilePath);
            }

            m_FileSuffixs = EditorGUILayout.TextField("文件后缀: ", m_FileSuffixs);

            m_FileEncoding = (EFileEncoding)EditorGUILayout.EnumPopup("文件编码格式：", m_FileEncoding);

            GUILayout.Space(5);

            if (GUILayout.Button($"\nCheck\n"))
            {
                var suffixs = m_FileSuffixs.Split(';');
                var suffixSet = new HashSet<string>(suffixs);
                var files = Directory.GetFiles(m_FilePath, "*", SearchOption.AllDirectories)
                    .Where(file =>
                    {
                        foreach (var item in suffixSet)
                        {
                            if (file.EndsWith(item))
                            {
                                return true;
                            }
                        }
                        return false;
                    });

                Debug.LogError(string.Join("\n", files));
            }

            if (GUILayout.Button($"\nConvert to {m_FileEncoding}\n"))
            {
                this.Conversion(GetEncodingByEnum(m_FileEncoding));
            }
        }

        // 开始转化
        private void Conversion(Encoding targetEncoding)
        {
            if (m_FilePath.Equals(string.Empty))
            {
                return;
            }

            string[] files = Directory.GetFiles(m_FilePath, "*", SearchOption.AllDirectories);

            if (EditorUtility.DisplayDialog("Sure?", "", "ok", "no"))
            {
                var suffixs = m_FileSuffixs.Split(';');
                var suffixSet = new HashSet<string>(suffixs);
                foreach (string file in files)
                {
                    foreach (var suffix in suffixSet)
                    {
                        if (file.EndsWith(suffix))
                        {
                            string strTempPath = file.Replace(@"\", "/");
                            ConvertFileEncoding(strTempPath, null, targetEncoding);
                        }
                    }
                }

                AssetDatabase.Refresh();
                Debug.Log("格式转换完成！");
            }
        }


        private static void ConvertFileEncoding(string srcFile, string destFile, Encoding targetEncoding)
        {
            destFile = string.IsNullOrEmpty(destFile) ? srcFile : destFile;
            var srcEncoding = FileEncodingUtil.GetEncoding(srcFile);
            File.WriteAllText(destFile, File.ReadAllText(srcFile, srcEncoding), targetEncoding);
            Debug.Log($"文件路径：{srcFile}  编码：{srcEncoding.BodyName}");
        }

        private static Encoding GetEncodingByEnum(EFileEncoding fileEncoding)
        {
            switch (fileEncoding)
            {
                case EFileEncoding.UTF8WithBOM:
                    return Encoding.UTF8;
                case EFileEncoding.UTF8:
                    return new UTF8Encoding(false);
                case EFileEncoding.Unicode:
                    return Encoding.Unicode;
                case EFileEncoding.BigEndianUnicode:
                    return Encoding.BigEndianUnicode;
                default:
                    throw new System.Exception("unhandle fileEncoding: " + fileEncoding);
            }
        }
    }
}
