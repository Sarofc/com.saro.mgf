using UnityEngine;

namespace Saro.XConsole
{
    [CreateAssetMenu(menuName = "MGF/" + nameof(Saro.XConsole) + "/" + nameof(Saro.XConsole.XConsoleSettings), fileName = nameof(Saro.XConsole.XConsoleSettings))]
    internal class XConsoleSettings : ScriptableObject
    {
        [Header("KeyCodes")]
        public KeyCode openKey = KeyCode.BackQuote;

        public KeyCode focusCmdInput = KeyCode.Tab;
        public KeyCode preCmdHistory = KeyCode.UpArrow;
        public KeyCode nextCmdHistory = KeyCode.DownArrow;

        [Header("Config")]
        public int listViewRefreshSpeed = 36;
        public float flushConsoleInterval = 0.05f;
    }
}