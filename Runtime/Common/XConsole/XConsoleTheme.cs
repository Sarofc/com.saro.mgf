using UnityEngine;

namespace Saro.XConsole
{
    [CreateAssetMenu(menuName = nameof(Saro.XConsole) + "/" + nameof(Saro.XConsole.XConsoleTheme), fileName = nameof(Saro.XConsole.XConsoleTheme))]
    internal class XConsoleTheme : ScriptableObject
    {
        public Sprite info;
        public Sprite warning;
        public Sprite error;

        public Color btnNormalColor = Color.white;
        public Color btnSelectedColor = Color.black;

        public Color lotItemColor1 = Color.gray;
        public Color lotItemColor2 = Color.clear;

        public Color textDefaultColor = Color.white;
        public Color suggestionTextHighlightColor = Color.yellow;

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