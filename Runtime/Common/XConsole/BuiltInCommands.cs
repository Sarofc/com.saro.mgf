namespace Saro.XConsole
{
    using System;
    using System.IO;
    using Saro.Utility;
    using UnityEngine;

    internal partial class BuiltInCommands
    {
        [XCommand("core.all_commands", "所有命令")]
        public static void all_commands()
        {
            var sb = StringBuilderCache.Get();
            foreach (var cmd in XConsole.GetAllCommands())
            {
                sb.AppendLine(cmd.ToString());
            }

            Log.INFO(StringBuilderCache.GetStringAndRelease(sb));
        }

        [XCommand("core.save_log", "保存日志")]
        public static void save_log()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, "log_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".txt");
                File.WriteAllText(path, XConsole.GetLog());

                Log.INFO("Save log file to : " + path);
            }
            catch (Exception e)
            {
                Log.ERROR(e.Message);
            }
        }

        [XCommand("core.clear_cmd_histories", "清理命令历史")]
        public static void clear_cmd_histories()
        {
            XConsole.ClearCommandHistory();
        }

        [XCommand("core.clear_log", "清除所有日志")]
        public static void clear_log()
        {
            XConsole.ClearLog();
        }
    }
}

#if UNITY_EDITOR

namespace Saro.XConsole
{
    using Cysharp.Threading.Tasks;

    internal partial class BuiltInCommands
    {
        [XCommand("core.test.generate_large_logs", "测试！生成大量log")]
        private static async void generate_large_logs()
        {
            for (int i = 0; i < 5000; i++)
            {
                Log.INFO("generate large logs: " + i);
                await UniTask.Delay(5);
            }
        }
    }
}

#endif