#if UNITY_EDITOR

using System;
using System.Linq;
using Saro.SEditor;
using UnityEditor;
using UnityEngine;

namespace Saro.SEditor
{
    [CustomPropertyDrawer(typeof(SelectTypeAttribute))]
    internal class SelectTypeDrawer : PropertyDrawer
    {
        private const string TYPE_ERROR =
            "SelectTypeAttribute can only be used on string fields.";

        private const string EMPTY = "*Empty";

        /// <summary>
        /// Cache of the hash to use to resolve the ID for the drawer.
        /// </summary>
        private int idHash;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // If this is not used on an string, show an error
            if (property.type != "string")
            {
                GUIStyle errorStyle = "CN EntryErrorIconSmall";
                Rect r = new Rect(position);
                r.width = errorStyle.fixedWidth;
                position.xMin = r.xMax;
                GUI.Label(r, "", errorStyle);
                GUI.Label(position, TYPE_ERROR);
                return;
            }

            // By manually creating the control ID, we can keep the ID for the
            // label and button the same. This lets them be selected together
            // with the keyboard in the inspector, much like a normal popup.
            if (idHash == 0) idHash = nameof(SelectTypeDrawer).GetHashCode();
            int id = GUIUtility.GetControlID(idHash, FocusType.Keyboard, position);

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, id, label);

            GUIContent buttonText;
            GUIStyle buttonStyle;
            if (!string.IsNullOrEmpty(property.stringValue))
            {
                buttonText = new GUIContent(property.stringValue);
                buttonStyle = EditorStyles.miniPullDown;
            }
            else
            {
                buttonText = new GUIContent(EMPTY);
                buttonStyle = new GUIStyle(EditorStyles.helpBox);
                buttonStyle.fontStyle = FontStyle.Bold;
            }

            if (SEditorUtility.DropdownButton(id, position, buttonText, buttonStyle))
            {
                var att = (attribute as SelectTypeAttribute);
                var supperType = att.SupperType;
                var types = Utility.TypeUtility.GetSubClassTypesAllAssemblies(supperType).Select(t => att.FullName ? t.FullName : t.Name).ToArray();
                ArrayUtility.Add(ref types, EMPTY);
                var index = Array.IndexOf(types, property.stringValue);
                if (index == -1) index = types.Length - 1;
                void onSelect(int i)
                {
                    property.stringValue = i == types.Length - 1 ? "" : types[i];
                    property.serializedObject.ApplyModifiedProperties();
                }

                SearchablePopup.Show(position, types, index, onSelect);
            }
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// A custom button drawer that allows for a controlID so that we can
        /// sync the button ID and the label ID to allow for keyboard
        /// navigation like the built-in enum drawers.
        /// </summary>
        private static bool DropdownButton(int id, Rect position, GUIContent content)
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
                    EditorStyles.popup.Draw(position, content, id, false);
                    break;
            }
            return false;
        }
    }
}

#endif