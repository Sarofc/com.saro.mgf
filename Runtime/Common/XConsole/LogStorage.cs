#if true

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Saro.XConsole
{
    // TODO 
    // 时间戳

    internal class LogStorage
    {
        /// <summary>
        /// 日志条目
        /// </summary>
        public class LogEntry : IEquatable<LogEntry>
        {
            private const int k_HASH_NOT_CALCULATED = -623218;
            private int m_Hash;

            public string logString;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            public string stackTrace;
#endif
            public UnityEngine.LogType logType;

            public int count;

            private static Stack<LogEntry> s_Pool = new();
            private static readonly object s_PoolLock = new();

            public static LogEntry Create(string logString, string stackTrace, UnityEngine.LogType logType)
            {
                LogEntry entry = null;

                lock (s_PoolLock)
                {
                    if (s_Pool.Count > 0)
                    {
                        entry = s_Pool.Pop();
                    }
                }

                if (entry != null)
                {
                    entry.logString = logString;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    entry.stackTrace = stackTrace.Length > 0 ? stackTrace.Remove(stackTrace.Length - 1, 1) : stackTrace;
#endif

                    entry.logType = logType;
                    entry.count = 1;

                    return entry;
                }

                return new LogEntry(logString, stackTrace, logType);
            }

            public static void Release(LogEntry entry)
            {
                if (entry == null)
                {
                    Log.ERROR("entry is null. can't release.");
                    return;
                }

                entry.logString = null;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                entry.stackTrace = null;
#endif

                entry.logType = 0;

                entry.m_Hash = k_HASH_NOT_CALCULATED;
                entry.count = 1;

                lock (s_PoolLock)
                {
                    s_Pool.Push(entry);
                }
            }

            private LogEntry(string logString, string stackTrace, UnityEngine.LogType logType)
            {
                this.logString = logString;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                this.stackTrace = stackTrace.Length > 0 ? stackTrace.Remove(stackTrace.Length - 1, 1) : stackTrace;
#endif
                this.logType = logType;

                this.count = 1;

                m_Hash = k_HASH_NOT_CALCULATED;
            }

            public string GetDetialLog()
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return logString + "\n" + stackTrace;
#else
                return logString;
#endif
            }

            [System.Diagnostics.Conditional("UNITY_EDITOR")]
            public void TraceScript()
            {
#if UNITY_EDITOR
                var regex = System.Text.RegularExpressions.Regex.Match(stackTrace, @"\(at .*\.cs:[0-9]+\)$", System.Text.RegularExpressions.RegexOptions.Multiline);
                if (regex.Success)
                {
                    string line = stackTrace.Substring(regex.Index + 4, regex.Length - 5);
                    int lineSeparator = line.IndexOf(':');

                    UnityEditor.MonoScript script = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(line.Substring(0, lineSeparator));
                    if (script != null)
                    {
                        UnityEditor.AssetDatabase.OpenAsset(script, int.Parse(line.Substring(lineSeparator + 1)));
                    }
                }
#endif
            }

            public bool Equals(LogEntry other)
            {
                return logString == other.logString
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    && stackTrace == other.stackTrace
#endif
                    ;
            }

            // Ovirride hash function to use this as Key for Dictionary
            // Credit: https://stackoverflow.com/a/19250516/2373034
            public override int GetHashCode()
            {
                if (m_Hash == k_HASH_NOT_CALCULATED)
                {
                    unchecked
                    {
                        m_Hash = 17;
                        m_Hash = m_Hash * m_Hash * 23 + logString == null ? 0 : logString.GetHashCode();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        m_Hash = m_Hash * m_Hash * 23 + stackTrace == null ? 0 : stackTrace.GetHashCode();
#endif
                    }
                }
                return m_Hash;
            }
        }

        /// <summary>
        /// 相同信息是否折叠
        /// </summary>
        public bool IsCollapsedEnable
        {
            get
            {
                return HasLogFlag(ELogTypeFlag.Collapsed);
            }
            set
            {
                if (value)
                {
                    SetLogFlag(ELogTypeFlag.Collapsed);
                }
                else
                {
                    UnsetLogFlag(ELogTypeFlag.Collapsed);
                }
            }
        }

        /// <summary>
        /// 显示时间戳
        /// </summary>
        public bool IsTimestampEnable
        {
            get
            {
                return HasLogFlag(ELogTypeFlag.Timestamp);
            }
            set
            {
                if (value)
                {
                    SetLogFlag(ELogTypeFlag.Timestamp);
                }
                else
                {
                    UnsetLogFlag(ELogTypeFlag.Timestamp);
                }
            }
        }

        /// <summary>
        /// 显示log
        /// </summary>
        public bool IsInfoEnable
        {
            get
            {
                return HasLogFlag(ELogTypeFlag.Info);
            }
            set
            {
                if (value)
                {
                    SetLogFlag(ELogTypeFlag.Info);
                }
                else
                {
                    UnsetLogFlag(ELogTypeFlag.Info);
                }
            }
        }

        /// <summary>
        /// 显示warning
        /// </summary>
        public bool IsWarningEnable
        {
            get => HasLogFlag(ELogTypeFlag.Warning);
            set
            {
                if (value)
                {
                    SetLogFlag(ELogTypeFlag.Warning);
                }
                else
                {
                    UnsetLogFlag(ELogTypeFlag.Warning);
                }
            }
        }

        /// <summary>
        /// 显示error
        /// </summary>
        public bool IsErrorEnable
        {
            get => HasLogFlag(ELogTypeFlag.Error);
            set
            {
                if (value)
                {
                    SetLogFlag(ELogTypeFlag.Error);
                }
                else
                {
                    UnsetLogFlag(ELogTypeFlag.Error);
                }
            }
        }

        public bool IsDebugAll => LogFlag == ELogTypeFlag.DebugAll;

        /// <summary>
        /// 日志flag
        /// </summary>
        [System.Flags]
        public enum ELogTypeFlag
        {
            None = 0,
            Error = 1 << 1,
            Warning = 1 << 2,
            Info = 1 << 3,

            Collapsed = 1 << 4,

            Timestamp = 1 << 5,

            DebugAll = Error | Warning | Info,

            All = Collapsed | Timestamp | DebugAll,
        }

        private Configs m_Configs;
        private ELogTypeFlag LogFlag { get => (ELogTypeFlag)m_Configs.logFlag; set => m_Configs.logFlag = (int)value; }

        /// <summary>
        /// 接收unity log，view需要监听
        /// </summary>
        public event Action<bool, int, int, int, int> LogMessageReceived;
        /// <summary>
        /// 折叠日志条目，每条都是唯一的，重复的不再添加进来
        /// </summary>
        public IReadOnlyList<LogEntry> CollapsedLogEntries => m_CollapsedLogEntries;
        /// <summary>
        /// 需要显示的日志条目索引，数据源 
        /// <see cref="CollapsedLogEntries"/>
        /// </summary>
        public IReadOnlyList<int> LogEntryIndicesToShow => m_LogEntryIndicesToShow;

        // store unique logentry
        private List<LogEntry> m_CollapsedLogEntries;
        /// <summary>
        /// logentry to index. see 
        /// <see cref="m_CollapsedLogEntries"/>
        /// </summary>
        private Dictionary<LogEntry, int> m_CollapsedLogEntriesMap;
        // uncollapsed list index
        private List<int> m_UnCollapsedLogEntryIndices;
        // logentry index to show
        private List<int> m_LogEntryIndicesToShow;

        private ConcurrentQueue<LogEntry> m_LogQueue;
        //private Queue<LogEntry> m_LogQueue;
        //private readonly object m_LogQueueLock = new object();

        private int m_InfoCount, m_WarningCount, m_ErrorCount;

        internal LogStorage(Configs configs)
        {
            m_Configs = configs;

            InitializeConfig();

            //Log.INFO("flag: " + m_LogFlag);

            UnityEngine.Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
        }

        private void InitializeConfig()
        {
            m_CollapsedLogEntries ??= new();
            m_CollapsedLogEntriesMap ??= new();
            m_UnCollapsedLogEntryIndices ??= new();
            m_LogEntryIndicesToShow ??= new();

            m_LogQueue ??= new();
        }

        internal void ClearLog()
        {
            for (int i = 0; i < m_CollapsedLogEntries.Count; i++)
            {
                var entry = m_CollapsedLogEntries[i];
                LogEntry.Release(entry);
            }

            m_CollapsedLogEntries.Clear();
            m_CollapsedLogEntriesMap.Clear();
            m_UnCollapsedLogEntryIndices.Clear();
            m_LogEntryIndicesToShow.Clear();

            m_InfoCount = m_WarningCount = m_ErrorCount = 0;

            LogMessageReceived?.Invoke(false, -1, m_InfoCount, m_WarningCount, m_ErrorCount);
        }

        /// <summary>
        /// log filter. see
        /// <see cref="ELogTypeFlag"/>
        /// </summary>
        internal void FilterLog()
        {
            m_LogEntryIndicesToShow.Clear();

            if (HasLogFlag(ELogTypeFlag.Collapsed))
            {
                for (int i = 0; i < m_CollapsedLogEntries.Count; i++)
                {
                    var entry = m_CollapsedLogEntries[i];
                    if (HasLogFlag(ELogTypeFlag.Info) && entry.logType == UnityEngine.LogType.Log ||
                        HasLogFlag(ELogTypeFlag.Warning) && entry.logType == UnityEngine.LogType.Warning ||
                        HasLogFlag(ELogTypeFlag.Error) && entry.logType == UnityEngine.LogType.Error)
                    {
                        m_LogEntryIndicesToShow.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < m_UnCollapsedLogEntryIndices.Count; i++)
                {
                    var entry = m_CollapsedLogEntries[m_UnCollapsedLogEntryIndices[i]];
                    if (HasLogFlag(ELogTypeFlag.Info) && entry.logType == UnityEngine.LogType.Log ||
                        HasLogFlag(ELogTypeFlag.Warning) && entry.logType == UnityEngine.LogType.Warning ||
                        HasLogFlag(ELogTypeFlag.Error) && entry.logType == UnityEngine.LogType.Error)
                    {
                        m_LogEntryIndicesToShow.Add(m_UnCollapsedLogEntryIndices[i]);
                    }
                }
            }
        }

        /// <summary>
        /// get log string
        /// <code>TODO: maybe chinese character cause error</code>
        /// </summary>
        /// <returns></returns>
        internal string GetLog()
        {
            int strLen = 100; // in case
            int newLineLen = Environment.NewLine.Length;

            for (int i = 0; i < m_UnCollapsedLogEntryIndices.Count; i++)
            {
                var entry = m_CollapsedLogEntries[m_UnCollapsedLogEntryIndices[i]];
                strLen += entry.logString.Length

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    + entry.stackTrace.Length
#endif

                    + newLineLen * 3;
            }

            var sb = StringBuilderCache.Get(strLen);
            for (int i = 0; i < m_UnCollapsedLogEntryIndices.Count; i++)
            {
                var entry = m_CollapsedLogEntries[m_UnCollapsedLogEntryIndices[i]];
                sb.AppendLine(entry.logString)

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    .AppendLine(entry.stackTrace)
#endif
                    ;

                sb.AppendLine();
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        /// <summary>
        /// receive unity log message. see 
        /// <see cref="UnityEngine.Application.logMessageReceivedThreaded"/>
        /// </summary>
        /// <param name="logString"></param>
        /// <param name="stackTrace"></param>
        /// <param name="logType"></param>
        private void Application_logMessageReceivedThreaded(string logString, string stackTrace, UnityEngine.LogType logType)
        {
            var entry = LogEntry.Create(logString, stackTrace, logType);

            m_LogQueue.Enqueue(entry);
        }

        /// <summary>
        /// call at lateupdate
        /// </summary>
        internal void ProcessLogQueue()
        {
            // 是否需要限制每帧处理的数量？？？
            if (m_LogQueue.TryDequeue(out var logEntry))
            {
                ProcessLog(logEntry);
            }
        }

        private void ProcessLog(LogEntry entry)
        {
            var has = m_CollapsedLogEntriesMap.TryGetValue(entry, out int index);

            //s_Timestamps.Add($"[{DateTime.Now.Ticks}]");
            var type = entry.logType;
            if (has)
            {
                m_CollapsedLogEntries[index].count++;
                LogEntry.Release(entry);

                //s_CollapsedTimestampsIndices[index] = s_Timestamps.Count - 1;
            }
            else
            {
                index = m_CollapsedLogEntries.Count;
                m_CollapsedLogEntries.Add(entry);

                m_CollapsedLogEntriesMap[entry] = index;

                //s_CollapsedTimestampsIndices.Add(index);
            }

            if (!(HasLogFlag(ELogTypeFlag.Collapsed) && has))
            {
                if (HasLogFlag(ELogTypeFlag.Error) ||
                    HasLogFlag(ELogTypeFlag.Warning) ||
                    HasLogFlag(ELogTypeFlag.Info))
                {
                    m_LogEntryIndicesToShow.Add(index);
                }
            }

            if (type == UnityEngine.LogType.Error) m_ErrorCount++;
            if (type == UnityEngine.LogType.Warning) m_WarningCount++;
            if (type == UnityEngine.LogType.Log) m_InfoCount++;

            m_UnCollapsedLogEntryIndices.Add(index);

            LogMessageReceived?.Invoke(has, index, m_InfoCount, m_WarningCount, m_ErrorCount);
        }

        internal int GetEntryIndexAtIndicesToShow(int entryIndex)
        {
            return m_LogEntryIndicesToShow.IndexOf(entryIndex);
        }

        private void SetLogFlag(ELogTypeFlag type)
        {
            LogFlag |= type;
        }

        private void UnsetLogFlag(ELogTypeFlag type)
        {
            LogFlag &= ~type;
        }

        private bool HasLogFlag(ELogTypeFlag type)
        {
            return (LogFlag & type) != 0;
        }
    }
}

#endif
