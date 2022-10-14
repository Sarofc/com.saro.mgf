using System;
using System.Collections;
using System.Collections.Generic;
using Saro.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Saro.XConsole
{
    internal partial class XConsole : MonoSingleton<XConsole>, ISuperScrollRectDataProvider
    {
        [InlineEditor]
        public XConsoleSettings settings;
        [InlineEditor]
        public XConsoleTheme theme;

        //public PersientConfigs configs;

        private CmdExecutor m_Executor;
        private LogStorage m_LogStorge;
        private Transform m_Window;


        public void ExecuteCommand(string commandLine)
        {
            m_Executor.ExecuteCommand(commandLine);
        }

        #region LogView

        private IReadOnlyList<int> IndexesToShow
        {
            get
            {
                if (InSearch) return m_SearchLogEntryIndexes;
                return m_LogStorge.LogEntryIndicesToShow;
            }
        }
        private List<int> m_SearchLogEntryIndexes = new();

        private bool InSearch => !string.IsNullOrEmpty(input_Search.text);
        public int GetCellCount()
        {
            return IndexesToShow.Count;
        }

        void ISuperScrollRectDataProvider.SetCell(GameObject cell, int index)
        {
            var entries = m_LogStorge.CollapsedLogEntries;
            var indexes = IndexesToShow;

            var logItem = cell.GetComponent<LogItem>();
            var logEntry = entries[indexes[index]];

            logItem.Refresh(this, logEntry, index, m_LogStorge.IsCollapsedEnable);
        }

        internal void UpdateDetialView(string content)
        {
            txt_DetailLog.text = content;
        }

        #endregion

        private UIBinder Binder { get; set; }

        private SuperScrollRect ListView => go_LogItemView.GetComponent<SuperScrollRect>();

        public bool RequireFlushToConsole { get; private set; }

        private bool m_Opened;

        private int m_ErrorCount;
        private int m_WarningCount;
        private int m_InfoCount;

        [Button(nameof(ApplyTheme))]
        private void ApplyTheme()
        {
            // button state
            SetImageState(go_ResizeButton.GetComponent<Image>(), false);
            SetButtonState(btn_Close, false);
            SetButtonState(btn_Clear, false);

            SetButtonState(btn_Collapse, m_LogStorge.IsCollapsedEnable);
            SetButtonState(btn_Info, m_LogStorge.IsInfoEnable);
            SetButtonState(btn_Warning, m_LogStorge.IsWarningEnable);
            SetButtonState(btn_Error, m_LogStorge.IsErrorEnable);

            txt_DetailLog.color = theme.textDefaultColor;
            txt_ErrorCount.color = theme.textDefaultColor;
            txt_InfoCount.color = theme.textDefaultColor;
            txt_WarningCount.color = theme.textDefaultColor;
            txt_Suggestion.color = theme.textDefaultColor;

            var logItem = go_LogItem.GetComponent<LogItem>();
            logItem.txt_LogCount.color = theme.textDefaultColor;
            logItem.txt_LogEntry.color = theme.textDefaultColor;
        }

        private void SetButtonState(Button button, bool state)
        {
            SetImageState(button.image, state);
        }

        private void SetImageState(Image image, bool state)
        {
            image.color = state ? theme.btnSelectedColor : theme.btnNormalColor;
        }

        #region Unity Method

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            Binder = GetComponent<UIBinder>();
            m_LogStorge = new LogStorage();
            m_Executor = new CmdExecutor();

            ListView.DoAwake(this);
            ListView.SetRefreshSpeed(36);

            m_Window = transform.Find("Root/Window");

            Register();

            ApplyTheme();
        }


        IEnumerator Start()
        {
            OpenWindow(false);

            yield return null;

            FilterLog();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Unregister();
        }

        private void LateUpdate()
        {
            m_LogStorge.ProcessLogQueue();

            FlushToConsole();

            ProcessKey();
        }

        private void OnApplicationQuit()
        {
            Log.INFO("----------------quit");

#if UNITY_EDITOR || UNITY_STANDALONE
            SaveSettings();
#endif
        }

        private void OnApplicationPause(bool pause)
        {
            Log.INFO("----------------pause");

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            SaveSettings();
#endif
        }

        private void Register()
        {
            btn_Close.onClick.AddListener(OnCloseClick);
            btn_Clear.onClick.AddListener(OnClearBtnClick);
            btn_Collapse.onClick.AddListener(OnCollapseBtnClick);

            btn_Info.onClick.AddListener(OnFilterInfoBtnClick);
            btn_Warning.onClick.AddListener(OnFilterWarningBtnClick);
            btn_Error.onClick.AddListener(OnFilterErrorBtnClick);

            input_Command.onValidateInput += OnValidateCommand;
            input_Command.onValueChanged.AddListener(OnChangedCommand);
            input_Search.onValueChanged.AddListener(UpdateSearch);

            m_LogStorge.LogMessageReceived += UpdateWindow;
        }

        private void Unregister()
        {
            //m_ResizeBtn.triggers.Clear();

            //btn_Close.onClick.RemoveListener(OnCloseClick);
            //btn_Clear.onClick.RemoveListener(OnClearBtnClick);
            //btn_Collapse.onClick.RemoveListener(OnCollapseBtbClick);

            //btn_Info.onClick.RemoveListener(OnFilterInfoBtnClick);
            //btn_Warning.onClick.RemoveListener(OnFilterWarningBtnClick);
            //btn_Error.onClick.RemoveListener(OnFilterErrorBtnClick);

            //input_Command.onValidateInput -= OnValidateCommand;
            //input_Command.onValueChanged.RemoveListener(OnChangedCommand);

            m_LogStorge.LogMessageReceived -= UpdateWindow;
        }

        private void SaveSettings()
        {
            Log.INFO("----------------save settings");
        }

        #endregion

        #region Window
        public void OpenWindow(bool show)
        {
            m_Window.gameObject.SetActive(show);
            if (show)
            {
                RequireFlushToConsole = true;
                UpdateTexts(m_InfoCount, m_WarningCount, m_ErrorCount, true);
            }
            else
            {
                input_Command.text = "";
                //EventSystem.current.SetSelectedGameObject(null);
            }
            m_Opened = show;
        }

        #endregion

        #region Log

        private void FlushToConsole()
        {
            if (RequireFlushToConsole && m_Opened)
            {
                RequireFlushToConsole = false;
                ListView.ReloadData();
            }
        }

        private void UpdateTexts(int infoCount, int warningCount, int errorCount, bool force = false)
        {
            if (m_WarningCount != warningCount || force)
            {
                m_WarningCount = warningCount;
                if (m_Opened || force)
                    txt_WarningCount.text = m_WarningCount.ToString();
            }

            if (m_InfoCount != infoCount || force)
            {
                m_InfoCount = infoCount;
                if (m_Opened || force)
                    txt_InfoCount.text = m_InfoCount.ToString();
            }

            if (m_ErrorCount != errorCount || force)
            {
                m_ErrorCount = errorCount;
                if (m_Opened || force)
                    txt_ErrorCount.text = m_ErrorCount.ToString();
            }
        }

        private void UpdateWindow(bool has, int index, int infoCount, int warningCount, int errorCount)
        {
            UpdateTexts(infoCount, warningCount, errorCount);

            // update force
            if (index == -1)
            {
                return;
            }

            RequireFlushToConsole = true;

            //if (ListView.verticalNormalizedPosition <= 0.15f)
            {
                //ListView.JumpTo(GetCellCount() - 1);
            }
        }

        private void FilterLog()
        {
            m_LogStorge.FilterLog();

            UpdateSearch(input_Search.text);

            RequireFlushToConsole = true;
        }

        #endregion

        #region Update

        private void ProcessKey()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            PcInput();
#elif UNITY_ANDROID || UNITY_IOS
            MobileInput();
#endif
        }

        private void MobileInput()
        {
            if (Input.touchCount == 4 && !m_Opened)
            {
                OpenWindow(m_Opened = !m_Opened);
            }
        }

        private void PcInput()
        {
            if (Input.GetKeyDown(settings.openKey))
            {
                OpenWindow(m_Opened = !m_Opened);
            }
            if (m_Opened && !input_Command.isFocused && Input.GetKeyDown(settings.focusCmdInput))
            {
                EventSystem.current.SetSelectedGameObject(null);
                input_Command.Select();
            }

            // command history
            if (m_Opened && input_Command.isFocused)
            {
                if (Input.GetKeyDown(settings.preCmdHistory))
                {
                    input_Command.text = m_Executor.GetPrevCommand();

                    input_Command.caretPosition = input_Command.text.Length;
                }
                else if (Input.GetKeyDown(settings.nextCmdHistory))
                {
                    input_Command.text = m_Executor.GetNextCommand();
                    input_Command.caretPosition = input_Command.text.Length;
                }
            }
        }

        #endregion

        #region Callback

        //--------------------------------------------
        // ui component
        //--------------------------------------------
        private char OnValidateCommand(string text, int charIndex, char addedChar)
        {
            // autocomplete
            // tab
            if (addedChar == '\t')
            {
                if (!string.IsNullOrEmpty(text))
                {
                    string command = m_Executor.AutoComplete();
                    if (!string.IsNullOrEmpty(command))
                    {
                        input_Command.onValidateInput -= OnValidateCommand;
                        input_Command.onValueChanged.RemoveListener(OnChangedCommand);

                        input_Command.text = command;
                        input_Command.caretPosition = input_Command.text.Length;

                        input_Command.onValidateInput += OnValidateCommand;
                        input_Command.onValueChanged.AddListener(OnChangedCommand);

                        UpdateSuggestionText(command);
                    }
                }
                return '\0';
            }
            // submit
            // enter
            else if (addedChar == '\n')
            {
                input_Command.onValidateInput -= OnValidateCommand;
                input_Command.text = "";
                input_Command.onValidateInput += OnValidateCommand;

                if (text.Length > 0)
                {
                    // Execute command
                    m_Executor.ExecuteCommand(text);

                    UpdateSuggestionText(null);
                }

                return '\0';
            }

            return addedChar;
        }

        private void UpdateSuggestionText(string newString)
        {
            if (string.IsNullOrEmpty(newString))
            {
                txt_Suggestion.text = string.Empty;
                LayoutRebuilder.ForceRebuildLayoutImmediate(txt_Suggestion.transform.parent as RectTransform);
                return;
            }

            var suggestiongList = m_Executor.GetSuggestionCommand();
            var sb = StringBuilderCache.Get(256);
            for (int i = 0; i < suggestiongList.Count; i++)
            {
                var cmd = suggestiongList[i];
                if (string.Equals(newString, cmd, System.StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendColorText(cmd, theme.suggestionTextHighlightColor);
                }
                else
                {
                    sb.Append(cmd);
                }
                if (i < suggestiongList.Count - 1) sb.AppendLine();
            }
            txt_Suggestion.text = StringBuilderCache.GetStringAndRelease(sb);
        }

        private void OnChangedCommand(string newString)
        {
            if (!string.IsNullOrEmpty(newString))
            {
                m_Executor.CollectSuggestionCommand(newString);
            }

            UpdateSuggestionText(newString);
        }

        private void UpdateSearch(string search)
        {
            m_SearchLogEntryIndexes.Clear();
            if (!string.IsNullOrEmpty(search))
            {
                var indexes = m_LogStorge.LogEntryIndicesToShow;
                var entries = m_LogStorge.CollapsedLogEntries;
                for (int i = 0; i < indexes.Count; i++)
                {
                    var index = indexes[i];
                    var entry = entries[index];
                    if (entry.logString.Contains(search, StringComparison.OrdinalIgnoreCase))
                    {
                        m_SearchLogEntryIndexes.Add(index);
                    }
                    // 目前stacktrace，也加进来
                    else if (entry.stackTrace.Contains(search, StringComparison.OrdinalIgnoreCase))
                    {
                        m_SearchLogEntryIndexes.Add(index);
                    }
                }
            }
            RequireFlushToConsole = true;
        }

        private void OnCloseClick()
        {
            OpenWindow(false);
        }

        private void OnFilterErrorBtnClick()
        {
            SetButtonState(btn_Error, m_LogStorge.IsErrorEnable = !m_LogStorge.IsErrorEnable);

            FilterLog();
        }

        private void OnFilterWarningBtnClick()
        {
            SetButtonState(btn_Warning, m_LogStorge.IsWarningEnable = !m_LogStorge.IsWarningEnable);

            FilterLog();
        }

        private void OnFilterInfoBtnClick()
        {
            SetButtonState(btn_Info, m_LogStorge.IsInfoEnable = !m_LogStorge.IsInfoEnable);

            FilterLog();
        }

        private void OnCollapseBtnClick()
        {
            SetButtonState(btn_Collapse, m_LogStorge.IsCollapsedEnable = !m_LogStorge.IsCollapsedEnable);

            FilterLog();
        }

        private void OnClearBtnClick()
        {
            UpdateTexts(0, 0, 0);

            m_LogStorge.ClearLog();

            UpdateSearch(input_Search.text);
        }

        #endregion
    }

    partial class XConsole
    {
        // =============================================
        // code generate between >>begin and <<end
        // don't modify this scope

        //>>begin
		public UnityEngine.UI.Button btn_Clear => Binder.GetRef<UnityEngine.UI.Button>("btn_Clear");
		public UnityEngine.UI.Button btn_Collapse => Binder.GetRef<UnityEngine.UI.Button>("btn_Collapse");
		public UnityEngine.UI.InputField input_Search => Binder.GetRef<UnityEngine.UI.InputField>("input_Search");
		public UnityEngine.UI.Button btn_Info => Binder.GetRef<UnityEngine.UI.Button>("btn_Info");
		public UnityEngine.UI.Text txt_InfoCount => Binder.GetRef<UnityEngine.UI.Text>("txt_InfoCount");
		public UnityEngine.UI.Button btn_Warning => Binder.GetRef<UnityEngine.UI.Button>("btn_Warning");
		public UnityEngine.UI.Text txt_WarningCount => Binder.GetRef<UnityEngine.UI.Text>("txt_WarningCount");
		public UnityEngine.UI.Button btn_Error => Binder.GetRef<UnityEngine.UI.Button>("btn_Error");
		public UnityEngine.UI.Text txt_ErrorCount => Binder.GetRef<UnityEngine.UI.Text>("txt_ErrorCount");
		public UnityEngine.UI.Button btn_Close => Binder.GetRef<UnityEngine.UI.Button>("btn_Close");
		public UnityEngine.GameObject go_LogItem => Binder.GetRef<UnityEngine.GameObject>("go_LogItem");
		public UnityEngine.GameObject go_LogItemView => Binder.GetRef<UnityEngine.GameObject>("go_LogItemView");
		public UnityEngine.UI.Text txt_DetailLog => Binder.GetRef<UnityEngine.UI.Text>("txt_DetailLog");
		public UnityEngine.UI.InputField input_Command => Binder.GetRef<UnityEngine.UI.InputField>("input_Command");
		public UnityEngine.UI.Text txt_Suggestion => Binder.GetRef<UnityEngine.UI.Text>("txt_Suggestion");
		public UnityEngine.GameObject go_ResizeButton => Binder.GetRef<UnityEngine.GameObject>("go_ResizeButton");

	//<<end

        // =============================================
    }
}