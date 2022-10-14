using System;
using Saro.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Saro.XConsole
{
    internal sealed partial class LogItem : MonoBehaviour
    {
        private UIBinder Binder { get; set; }
        private LogStorage.LogEntry m_LogEntry;
        private XConsole m_Console;

        private void Awake()
        {
            Binder = GetComponent<UIBinder>();
            btn_bg.onClick.AddListener(() =>
            {
                m_Console.UpdateDetialView(m_LogEntry.GetDetialLog());
            });
        }

        internal void Refresh(XConsole console, LogStorage.LogEntry logEntry, int index, bool collapsed)
        {
            m_Console = console;
            m_LogEntry = logEntry;

            img_LogType.sprite = m_Console.theme.GetSprite(m_LogEntry.logType);
            txt_LogEntry.text = logEntry.logString;
            txt_LogCount.text = logEntry.count.ToString();
            txt_LogCount.transform.parent.gameObject.SetActive(collapsed);

            btn_bg.GetComponent<Image>().color = index % 2 == 0 ? m_Console.theme.lotItemColor1 : m_Console.theme.lotItemColor2;
        }
    }

    partial class LogItem
    {
        // =============================================
        // code generate between >>begin and <<end
        // don't modify this scope

        //>>begin
        public UnityEngine.UI.Button btn_bg => Binder.GetRef<UnityEngine.UI.Button>("btn_bg");
        public UnityEngine.UI.Image img_LogType => Binder.GetRef<UnityEngine.UI.Image>("img_LogType");
        public UnityEngine.UI.Text txt_LogEntry => Binder.GetRef<UnityEngine.UI.Text>("txt_LogEntry");
        public UnityEngine.UI.Text txt_LogCount => Binder.GetRef<UnityEngine.UI.Text>("txt_LogCount");

        //<<end

        // =============================================
    }
}
