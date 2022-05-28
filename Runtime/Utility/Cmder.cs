#if UNITY_EDITOR

using System;
using System.Diagnostics;

namespace Saro.Common
{
    public static class Cmder
    {

        public static void Run(string file, bool createWindow = false)
        {
            using (var proc = new Process())
            {
                proc.StartInfo.CreateNoWindow = !createWindow;
                proc.StartInfo.FileName = file;
                proc.StartInfo.UseShellExecute = false;

                proc.Start();
            }
        }

        public static void RunCmd(string cmd)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = "/k" + cmd,
                    CreateNoWindow = false,
                }
            };
            try
            {
                process.Start();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
            finally
            {
                process.Close();
            }
        }
    }
}

#endif