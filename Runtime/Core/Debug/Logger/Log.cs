using System.Collections.Generic;

namespace Saro
{
    /*
     * TODO 线程不安全，看怎么重构下
     */
    public static class Log
    {
        public const string k_ScriptDefineSymbol = "ENABLE_LOG";

        private const string k_DefaultKey = "Default";

        private static readonly Dictionary<string, ILogger> s_LoggerMap = new();

        [System.Diagnostics.Conditional(k_ScriptDefineSymbol)]
        public static void INFO(string msg)
        {
            EnsureDefaultLogger(k_DefaultKey);
            s_LoggerMap[k_DefaultKey].INFO(msg);
        }

        [System.Diagnostics.Conditional(k_ScriptDefineSymbol)]
        public static void INFO(string key, string message)
        {
            EnsureDefaultLogger(key);
            s_LoggerMap[key].INFO($"[{key}] {message}");
        }

        [System.Diagnostics.Conditional(k_ScriptDefineSymbol)]
        public static void INFO(string key, string message, string color)
        {
            EnsureDefaultLogger(key);
            s_LoggerMap[key].INFO($"<color={color}>[{key}]</color> {message}");
        }


        [System.Diagnostics.Conditional(k_ScriptDefineSymbol)]
        public static void WARN(string msg)
        {
            EnsureDefaultLogger(k_DefaultKey);
            s_LoggerMap[k_DefaultKey].WARN(msg);
        }

        [System.Diagnostics.Conditional(k_ScriptDefineSymbol)]
        public static void WARN(string key, string message)
        {
            EnsureDefaultLogger(key);
            s_LoggerMap[key].WARN($"[{key}] {message}");
        }

        [System.Diagnostics.Conditional(k_ScriptDefineSymbol)]
        public static void WARN(string key, string message, string color)
        {
            EnsureDefaultLogger(key);
            s_LoggerMap[key].WARN($"<color={color}>[{key}]</color> {message}");
        }

        public static void ERROR(string msg)
        {
            EnsureDefaultLogger(k_DefaultKey);
            s_LoggerMap[k_DefaultKey].ERROR(msg);
        }

        public static void ERROR(System.Exception e)
        {
            EnsureDefaultLogger(k_DefaultKey);
            s_LoggerMap[k_DefaultKey].ERROR(e);
        }
        
        [System.Diagnostics.Conditional(k_ScriptDefineSymbol)]
        public static void Assert(bool condition, string message)
        {
            EnsureDefaultLogger(k_DefaultKey);
            s_LoggerMap[k_DefaultKey].Assert(condition, message);
        }

        public static void ERROR(string key, System.Exception e)
        {
            EnsureDefaultLogger(key);
            s_LoggerMap[key].ERROR($"[{key}] {e}");
        }

        public static void ERROR(string key, string message)
        {
            EnsureDefaultLogger(key);
            s_LoggerMap[key].ERROR($"[{key}] {message}");
        }

        public static void ERROR(string key, string message, string color)
        {
            EnsureDefaultLogger(key);
            s_LoggerMap[key].ERROR($"<color={color}>[{key}]</color> {message}");
        }

        public static void AddLogger(string key, ILogger logger)
        {
            s_LoggerMap.Add(key, logger);
        }

        public static void RemoveLogger(string key)
        {
            s_LoggerMap.Remove(key);
        }

        private static void EnsureDefaultLogger(string key)
        {
            if (s_LoggerMap.ContainsKey(key)) return;

            AddLogger(key, new DefaultUnityLogger());
        }
    }
}