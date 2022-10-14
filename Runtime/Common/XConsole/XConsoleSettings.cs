using UnityEngine;

namespace Saro.XConsole
{
    [CreateAssetMenu(menuName = nameof(Saro.XConsole) + "/" + nameof(Saro.XConsole.XConsoleSettings), fileName = nameof(Saro.XConsole.XConsoleSettings))]
    internal class XConsoleSettings : ScriptableObject
    {
        [Header("key")]
        public KeyCode openKey = KeyCode.BackQuote;

        public KeyCode focusCmdInput = KeyCode.Tab;
        public KeyCode preCmdHistory = KeyCode.UpArrow;
        public KeyCode nextCmdHistory = KeyCode.DownArrow;
    }
}