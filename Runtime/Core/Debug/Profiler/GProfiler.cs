using System.Diagnostics;

namespace Saro.Diagnostics
{
    public static class GProfiler
    {
        [Conditional("ENABLE_PROFILER")]
        public static void BeginSample(string name)
        {
            UnityEngine.Profiling.Profiler.BeginSample(name);
        }

        [Conditional("ENABLE_PROFILER")]
        public static void EndSample()
        {
            UnityEngine.Profiling.Profiler.EndSample();
        }

        [Conditional("ENABLE_PROFILER")]
        public static void BeginThreadProfiling(string threadGroupName, string threadName)
        {
            UnityEngine.Profiling.Profiler.BeginThreadProfiling(threadGroupName, threadName);
        }

        [Conditional("ENABLE_PROFILER")]
        public static void EndThreadProfiling()
        {
            UnityEngine.Profiling.Profiler.EndThreadProfiling();
        }
    }
}
