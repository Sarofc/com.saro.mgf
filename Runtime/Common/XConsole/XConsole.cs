using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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

        [ReadOnly]
        public Configs configs;

        private CmdExecutor m_Executor;
        private LogStorage m_LogStorge;
        private Transform m_Window;

        #region API

        public static void AddInstanceCommand(Type classType, object instance)
        {
            Instance.m_Executor.AddInstanceCommand(classType, instance);
        }

        public static void AddStaticCommand(Type classType)
        {
            Instance.m_Executor.AddStaticCommand(classType);
        }

        public static void RemoveCommand(string cmd)
        {
            Instance.m_Executor.RemoveCommand(cmd);
        }

        public static void ExecuteCommand(string cmdLine)
        {
            Instance.m_Executor.ExecuteCommand(cmdLine);
        }

        public static void RegisterArgTypeParser(Type type, TypeParser parser)
        {
            Instance.m_Executor.RegisterArgTypeParser(type, parser);
        }

        public static IReadOnlyCollection<CmdData> GetAllCommands()
        {
            return Instance.m_Executor.GetAllCommands();
        }

        public static void ClearCommandHistory()
        {
            Instance.m_Executor.ClearCommandHistory();
        }

        public static void ClearLog()
        {
            Instance.m_LogStorge.ClearLog();
        }

        public static string GetLog()
        {
            return Instance.m_LogStorge.GetLog();
        }

        #endregion

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

        internal int CurrentSelectIndex { get; private set; } = -1;

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

        internal void UpdateDetialView(string content, int index)
        {
            txt_DetailLog.text = content;
            CurrentSelectIndex = index;

            RequireFlushToConsole = true;

            txt_DetailLog.SetLayoutDirty();
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
            //SetImageState(go_ResizeButton.GetComponent<Image>(), false);

            img_icon_Info.sprite = theme.GetSprite(LogType.Log);
            img_icon_Warning.sprite = theme.GetSprite(LogType.Warning);
            img_icon_Error.sprite = theme.GetSprite(LogType.Error);
            img_icon_ResizeButton.sprite = theme.resize;

            go_TopBar.GetComponent<Image>().color = theme.bgTopBarColor;
            go_LogItemView.GetComponent<Image>().color = theme.bgLogItemViewColor;
            go_LogDetialView.GetComponent<Image>().color = theme.bgLogDetialViewColor;
            go_BottomBar.GetComponent<Image>().color = theme.bgBottomBarColor;

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

            img_bg_Suggestion.color = theme.bgSuggestionColor;
            txt_Suggestion.color = theme.textDefaultColor;

            var logItem = go_LogItem.GetComponent<LogItem>();
            logItem.txt_LogCount.color = theme.textDefaultColor;
            logItem.txt_LogEntry.color = theme.textDefaultColor;
            logItem.txt_LogCount.transform.parent.GetComponent<Image>().color = theme.logItemBgCountColor;

            input_Search.GetComponent<Image>().color = theme.bgInputSearchColor;
            input_Command.GetComponent<Image>().color = theme.bgInputCommandColor;
        }

        private void SetButtonState(Button button, bool state)
        {
            SetImageState(button.image, state);
        }

        private void SetImageState(Image image, bool state)
        {
            image.color = state ? theme.btnSelectedColor : theme.btnNormalColor;
        }

        private const string k_ConfigFile = "saro_xconsole_config";
        private void LoadConfigs()
        {
            if (PlayerPrefs.HasKey(k_ConfigFile))
            {
                var json = PlayerPrefs.GetString(k_ConfigFile);
                var obj = JsonConvert.DeserializeObject<Configs>(json);
                configs = obj;
            }
            else
            {
                configs = new Configs();
            }
        }

        private void SaveConfigs()
        {
            var json = JsonConvert.SerializeObject(configs);
            PlayerPrefs.SetString(k_ConfigFile, json);
        }

        #region Unity Method

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);

            Binder = GetComponent<UIBinder>();

            LoadConfigs();

            m_LogStorge = new LogStorage(configs);
            m_Executor = new CmdExecutor(configs);
            go_ResizeButton.GetComponent<ResizeButton>().configs = configs;

            ListView.DoAwake(this);
            ListView.SetRefreshSpeed(settings.listViewRefreshSpeed);

            m_Window = transform.Find("Root/Window");

            Register();

            ApplyTheme();
        }


        void Start()
        {
            UpdateSuggestionText(null);
            OpenWindow(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Unregister();
        }

        private float m_FlushTimer = 0f;
        private void LateUpdate()
        {
            ProcessKey();

            m_LogStorge.ProcessLogQueue();

            if (m_FlushTimer >= settings.flushConsoleInterval)
            {
                FlushToConsole();
                m_FlushTimer -= settings.flushConsoleInterval;
            }
            else
            {
                m_FlushTimer += Time.deltaTime;
            }
        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            SaveConfigs();
#endif
        }

        private void OnApplicationPause(bool pause)
        {
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
            m_LogStorge.LogMessageReceived -= UpdateWindow;
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
                FilterLog();
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

                // snap to bottom
                if (ListView.verticalNormalizedPosition <= 0.1f)
                {
                    ListView.verticalNormalizedPosition = 0f;
                    //ListView.JumpTo(((ISuperScrollRectDataProvider)this).GetCellCount() - 1);
                }
            }
        }

        private void UpdateTexts(int infoCount, int warningCount, int errorCount, bool force = false)
        {
            if (m_WarningCount != warningCount || force)
            {
                m_WarningCount = warningCount;
                if (m_Opened || force)
                    txt_WarningCount.text = m_WarningCount > 999 ? "99+" : m_WarningCount.ToString();
            }

            if (m_InfoCount != infoCount || force)
            {
                m_InfoCount = infoCount;
                if (m_Opened || force)
                    txt_InfoCount.text = m_InfoCount > 999 ? "99+" : m_InfoCount.ToString();
            }

            if (m_ErrorCount != errorCount || force)
            {
                m_ErrorCount = errorCount;
                if (m_Opened || force)
                    txt_ErrorCount.text = m_ErrorCount > 999 ? "99+" : m_ErrorCount.ToString();
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

            UpdateDetialView(string.Empty, -1);

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
            var suggestionText = string.Empty;

            if (!string.IsNullOrEmpty(newString))
            {
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
                suggestionText = StringBuilderCache.GetStringAndRelease(sb);
            }

            if (string.IsNullOrEmpty(suggestionText))
            {
                if (img_bg_Suggestion.gameObject.activeSelf == true)
                    img_bg_Suggestion.gameObject.SetActive(false);
            }
            else
            {
                if (img_bg_Suggestion.gameObject.activeSelf == false)
                    img_bg_Suggestion.gameObject.SetActive(true);

                txt_Suggestion.SetLayoutDirty();
            }

            txt_Suggestion.text = suggestionText;
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

            UpdateDetialView(string.Empty, -1);
        }

        #endregion
    }

    partial class XConsole
    {
        // =============================================
        // code generate between >>begin and <<end
        // don't modify this scope

        //>>begin
        public UnityEngine.GameObject go_TopBar => Binder.GetRef<UnityEngine.GameObject>("go_TopBar");
        public UnityEngine.UI.Button btn_Clear => Binder.GetRef<UnityEngine.UI.Button>("btn_Clear");
        public UnityEngine.UI.Button btn_Collapse => Binder.GetRef<UnityEngine.UI.Button>("btn_Collapse");
        public UnityEngine.UI.InputField input_Search => Binder.GetRef<UnityEngine.UI.InputField>("input_Search");
        public UnityEngine.UI.Button btn_Info => Binder.GetRef<UnityEngine.UI.Button>("btn_Info");
        public UnityEngine.UI.Image img_icon_Info => Binder.GetRef<UnityEngine.UI.Image>("img_icon_Info");
        public UnityEngine.UI.Text txt_InfoCount => Binder.GetRef<UnityEngine.UI.Text>("txt_InfoCount");
        public UnityEngine.UI.Button btn_Warning => Binder.GetRef<UnityEngine.UI.Button>("btn_Warning");
        public UnityEngine.UI.Image img_icon_Warning => Binder.GetRef<UnityEngine.UI.Image>("img_icon_Warning");
        public UnityEngine.UI.Text txt_WarningCount => Binder.GetRef<UnityEngine.UI.Text>("txt_WarningCount");
        public UnityEngine.UI.Button btn_Error => Binder.GetRef<UnityEngine.UI.Button>("btn_Error");
        public UnityEngine.UI.Image img_icon_Error => Binder.GetRef<UnityEngine.UI.Image>("img_icon_Error");
        public UnityEngine.UI.Text txt_ErrorCount => Binder.GetRef<UnityEngine.UI.Text>("txt_ErrorCount");
        public UnityEngine.UI.Button btn_Close => Binder.GetRef<UnityEngine.UI.Button>("btn_Close");
        public UnityEngine.GameObject go_LogItemView => Binder.GetRef<UnityEngine.GameObject>("go_LogItemView");
        public UnityEngine.GameObject go_LogItem => Binder.GetRef<UnityEngine.GameObject>("go_LogItem");
        public UnityEngine.GameObject go_LogDetialView => Binder.GetRef<UnityEngine.GameObject>("go_LogDetialView");
        public UnityEngine.UI.Text txt_DetailLog => Binder.GetRef<UnityEngine.UI.Text>("txt_DetailLog");
        public UnityEngine.GameObject go_BottomBar => Binder.GetRef<UnityEngine.GameObject>("go_BottomBar");
        public UnityEngine.UI.InputField input_Command => Binder.GetRef<UnityEngine.UI.InputField>("input_Command");
        public UnityEngine.UI.Image img_bg_Suggestion => Binder.GetRef<UnityEngine.UI.Image>("img_bg_Suggestion");
        public UnityEngine.UI.Text txt_Suggestion => Binder.GetRef<UnityEngine.UI.Text>("txt_Suggestion");
        public UnityEngine.GameObject go_ResizeButton => Binder.GetRef<UnityEngine.GameObject>("go_ResizeButton");
        public UnityEngine.UI.Image img_icon_ResizeButton => Binder.GetRef<UnityEngine.UI.Image>("img_icon_ResizeButton");

        //<<end

        // =============================================
    }
}