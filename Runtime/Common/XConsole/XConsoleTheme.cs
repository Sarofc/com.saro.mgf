using UnityEngine;

namespace Saro.XConsole
{
    [CreateAssetMenu(menuName = "MGF/" + nameof(Saro.XConsole) + "/" + nameof(Saro.XConsole.XConsoleTheme), fileName = nameof(Saro.XConsole.XConsoleTheme))]
    internal class XConsoleTheme : ScriptableObject
    {
        [Header("Sprites")]
        public Sprite info;
        public Sprite warning;
        public Sprite error;
        public Sprite resize;

        [Header("Colors")]
        public Color textDefaultColor = Color.white;

        public Color btnNormalColor = Color.white;
        public Color btnSelectedColor = Color.black;

        [Space]
        public Color lotItemColor1 = Color.gray;
        public Color lotItemColor2 = Color.clear;
        public Color logItemSelectColor = Color.blue;
        public Color logItemBgCountColor = Color.black;

        [Space]
        public Color suggestionTextHighlightColor = Color.yellow;
        public Color bgSuggestionColor = new(0, 0, 0, 0.5f);

        [Space]
        public Color bgBottomBarColor = new(0, 0, 0, 0.5f);
        public Color bgLogItemViewColor = new(0, 0, 0, 0.5f);
        public Color bgLogDetialViewColor = new(0, 0, 0, 0.5f);
        public Color bgTopBarColor = new(0, 0, 0, 0.5f);

        [Space]
        public Color bgInputSearchColor = Color.gray;
        public Color bgInputCommandColor = Color.gray;

        public Sprite GetSprite(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    return error;
                case LogType.Warning:
                    return warning;
                case LogType.Log:
                    return info;
                default:
                    break;
            }
            return null;
        }
    }
}