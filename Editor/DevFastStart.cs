#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Saro.Dev
{
    // TODO 推出播放后，还原播放前的场景
    public sealed class DevFastStartSO : ScriptableObject
    {
        public const string k_ConfigAssetPath = "ProjectSettings/DevFastStart.json";

        public List<Entry> scenes = new List<Entry>();

        [Serializable]
        public class Entry
        {
            public string displayName;
            public string scenePath;
        }

#if ODIN_INSPECTOR
        [Button("Save")]
        [BoxGroup("Save")]
#endif
        public void Save()
        {
            var json = JsonUtility.ToJson(this);
            File.WriteAllText(k_ConfigAssetPath, json);
        }

        public static DevFastStartSO GetOrCreate()
        {
            var settings = ScriptableObject.CreateInstance<DevFastStartSO>();

            if (!File.Exists(k_ConfigAssetPath))
            {
                var defaultJson = JsonUtility.ToJson(settings);
                File.WriteAllText(k_ConfigAssetPath, defaultJson);
            }

            var json = File.ReadAllText(k_ConfigAssetPath);

            JsonUtility.FromJsonOverwrite(json, settings);

            return settings;
        }
    }

#if !ODIN_INSPECTOR
    [CustomEditor(typeof(DevFastStartSO))]
    public sealed class DevFastStartSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Save"))
            {
                (target as DevFastStartSO).Save();
            }
        }
    }
#endif

    [InitializeOnLoad]
    public class DevFastStart
    {
        private static DevFastStartSO s_Settings;

        private static class ToolbarStyles
        {
            public static readonly GUIStyle s_CommandButtonStyle;

            static ToolbarStyles()
            {
                s_CommandButtonStyle = new GUIStyle("AppCommand")
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        private static DevFastStartSO GetSettings()
        {
            if (s_Settings == null)
                s_Settings = DevFastStartSO.GetOrCreate();

            return s_Settings;
        }

        [SettingsProvider]
        private static SettingsProvider GetGameplayTagSettingsProvider()
        {
            var provider = AssetSettingsProvider.CreateProviderFromObject("Gameplay/DevFastStart", GetSettings());
            return provider;
        }

        static DevFastStart()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            var disable = (EditorApplication.isPlaying || EditorApplication.isPaused ||
                EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode);

            s_Settings = GetSettings();

            if (s_Settings.scenes == null || s_Settings.scenes.Count <= 0) return;

            var tmpEnable = GUI.enabled;
            GUI.enabled = !disable;

            for (int i = 0; i < s_Settings.scenes.Count; i++)
            {
                var scene = s_Settings.scenes[i];
                if (GUILayout.Button(new GUIContent($"{scene.displayName}", $"Open the [{scene.scenePath}] scene then Play"), ToolbarStyles.s_CommandButtonStyle))
                {
                    FastEnterGame(scene.scenePath);
                }
            }

            GUI.enabled = tmpEnable;
        }

        private static void FastEnterGame(string sceneToOpen)
        {
            //Debug.LogError($"Couldn't open scene. Path: {sceneToOpen}. Please modify {nameof(DevFastStart)}.{nameof(DevFastStart.s_MainSceneFunc)}");

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // 1. 打开 启动场景
                var currentScene = EditorSceneManager.OpenScene(sceneToOpen, OpenSceneMode.Additive);

                // 2. 关闭 启动场景 以外所有场景
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    var scene = EditorSceneManager.GetSceneAt(i);
                    if (scene != currentScene)
                    {
                        EditorSceneManager.CloseScene(scene, false);
                    }
                }

                EditorApplication.isPlaying = true;
            }
        }
    }
}

#endif