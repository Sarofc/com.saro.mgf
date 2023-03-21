using UnityEditor;
using UnityEngine;

namespace Saro.SEditor
{
    public static partial class SEditorUtility
    {
        public static GUIStyle TitleCentered => s_TitleCentered ??= new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
        private static GUIStyle s_TitleCentered;
        public static GUIStyle SmallTickbox => s_SmallTickbox ??= new GUIStyle("ShurikenToggle");
        private static GUIStyle s_SmallTickbox;
        private static readonly Color _splitterdark = new Color(0.12f, 0.12f, 0.12f, 1.333f);
        private static readonly Color _splitterlight = new Color(0.6f, 0.6f, 0.6f, 1.333f);
        public static Color Splitter { get { return EditorGUIUtility.isProSkin ? _splitterdark : _splitterlight; } }

        private static readonly Color _headerbackgrounddark = new Color(0.1f, 0.1f, 0.1f, 0.2f);
        private static readonly Color _headerbackgroundlight = new Color(1f, 1f, 1f, 0.4f);
        public static Color HeaderBackground { get { return EditorGUIUtility.isProSkin ? _headerbackgrounddark : _headerbackgroundlight; } }

        private static readonly Color _reorderdark = new Color(1f, 1f, 1f, 0.2f);
        private static readonly Color _reorderlight = new Color(0.1f, 0.1f, 0.1f, 0.2f);
        public static Color Reorder { get { return EditorGUIUtility.isProSkin ? _reorderdark : _reorderlight; } }

        private static readonly Texture2D _paneoptionsicondark;
        private static readonly Texture2D _paneoptionsiconlight;
        public static Texture2D PaneOptionsIcon { get { return EditorGUIUtility.isProSkin ? _paneoptionsicondark : _paneoptionsiconlight; } }

        static SEditorUtility()
        {
            _paneoptionsicondark = (Texture2D)EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
            _paneoptionsiconlight = (Texture2D)EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");
        }

        /// <summary>
        /// Simply drow a splitter line and a title bellow
        /// </summary>
        public static void DrawSection(string title)
        {
            EditorGUILayout.Space();

            DrawSplitter();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        /// <summary>
        /// Draw a separator line
        /// </summary>
        public static void DrawSplitter()
        {
            // Helper to draw a separator line

            var rect = GUILayoutUtility.GetRect(1f, 1f);

            rect.xMin = 0f;
            rect.width += 4f;

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, Splitter);
        }

        public static bool DropdownButton(int id, Rect position, GUIContent content, GUIStyle style)
        {
            Event current = Event.current;
            switch (current.type)
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && current.button == 0)
                    {
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == id && current.character == '\n')
                    {
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.Repaint:
                    style.Draw(position, content, id, false);
                    break;
            }
            return false;
        }

        public static string SearchField(string search)
        {
            GUILayout.BeginHorizontal();
            search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);
            if (!string.IsNullOrEmpty(search) && GUILayout.Button(string.Empty, EditorStyles.miniButton))
            {
                search = string.Empty;
                GUIUtility.keyboardControl = 0;
            }
            GUILayout.EndHorizontal();
            return search;
        }
    }
}