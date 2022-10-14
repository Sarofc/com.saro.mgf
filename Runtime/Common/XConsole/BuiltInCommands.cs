#if true

using System;
using System.IO;
using UnityEngine;

namespace Saro.XConsole
{
    internal partial class BuiltInCommands : ICommandRegister
    {
        [Command("core.all_commands", "所有命令")]
        public static void all_commands()
        {
            /*
            var sb = StringBuilderCache.Get();
            foreach (Command v in XConsoleGUI.Instance.GetAllCommands())
            {
                sb.AppendLine(v.ToString());
            }

            Log.INFO(StringBuilderCache.GetStringAndRelease(sb));
       */
        }

        [Command("core.save_log", "保存日志")]
        public static void save_log()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".txt");
                //File.WriteAllText(path, Terminal.Console.GetLog());

                Log.INFO("Save log file to : " + path);
            }
            catch (Exception e)
            {
                Log.ERROR(e.Message);
            }
        }

        [Command("core.clear_cmd_histories", "清理命令历史")]
        public static void clear_cmd_histories()
        {
            //XConsoleGUI.Instance.ClearCommandHistory();
        }

        [Command("core.clear_log", "清除所有日志")]
        public static void clear_log()
        {

            //Terminal.Console.ClearLog();
        }
    }
}

#endif