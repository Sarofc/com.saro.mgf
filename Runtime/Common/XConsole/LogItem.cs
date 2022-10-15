using System;
using Saro.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Saro.XConsole
{
    internal sealed partial class LogItem : MonoBehaviour
    {
        [SerializeField]
        private UIBinder m_Binder;
        private LogStorage.LogEntry m_LogEntry;
        private XConsole m_Console;
        private int m_Index;

        private void Awake()
        {
            btn_bg.onClick.AddListener(() =>
            {
                m_Console.UpdateDetialView(m_LogEntry.GetDetialLog(), m_Index);
            });
        }

        internal void Refresh(XConsole console, LogStorage.LogEntry logEntry, int index, bool collapsed)
        {
            m_Console = console;
            m_LogEntry = logEntry;
            m_Index = index;

            img_LogType.sprite = m_Console.theme.GetSprite(m_LogEntry.logType);
            txt_LogEntry.text = logEntry.logString;
            txt_LogCount.text = logEntry.count > 99 ? "9+" : logEntry.count.ToString();
            txt_LogCount.transform.parent.gameObject.SetActive(collapsed);

            ColoredItem(m_Console, m_Index);
        }

        private void ColoredItem(XConsole console, int index)
        {
            if (console.CurrentSelectIndex == index)
            {
                btn_bg.GetComponent<Image>().color = console.theme.logItemSelectColor;
            }
            else
            {
                btn_bg.GetComponent<Image>().color = index % 2 == 0 ? console.theme.lotItemColor1 : console.theme.lotItemColor2;
            }

        }
    }

    partial class LogItem
    {
        // =============================================
        // code generate between >>begin and <<end
        // don't modify this scope

        //>>begin
        public UnityEngine.UI.Button btn_bg => m_Binder.GetRef<UnityEngine.UI.Button>("btn_bg");
        public UnityEngine.UI.Image img_LogType => m_Binder.GetRef<UnityEngine.UI.Image>("img_LogType");
        public UnityEngine.UI.Text txt_LogEntry => m_Binder.GetRef<UnityEngine.UI.Text>("txt_LogEntry");
        public UnityEngine.UI.Text txt_LogCount => m_Binder.GetRef<UnityEngine.UI.Text>("txt_LogCount");

        //<<end

        // =============================================
    }
}
