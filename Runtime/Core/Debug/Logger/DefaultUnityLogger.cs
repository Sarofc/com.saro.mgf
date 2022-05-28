using System;

namespace Saro
{
    public sealed class DefaultUnityLogger : ILogger
    {
        // TODO log file

        public void INFO(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void WARN(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public void ERROR(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public void ERROR(Exception exception)
        {
            UnityEngine.Debug.LogError(exception);
        }

        public void Assert(bool condition, string message)
        {
            UnityEngine.Debug.Assert(condition, message);
        }
    }
}