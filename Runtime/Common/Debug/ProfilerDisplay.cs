namespace Saro.Profiler
{
    using System;
    using System.Diagnostics;
    using UnityEngine;
    using UnityEngine.Profiling;

    public sealed class ProfilerDisplay : MonoBehaviour
    {
        private string m_FpsText;
        private string m_HeapSizeText;
        private string m_UsedSizeText;
        private string m_AllocatedMemoryText;
        private string m_ReservedMemoryText;
        private string m_UnusedReservedMemoryText;
        private string m_CpuText;

        private int m_Index = 1;
        private int m_IndexCount = 100;
        private int m_FramesIndex;

        private float m_Fps;
        private float m_CurTime;
        private float m_LastTime;
        private float m_FpsDelay;

        private TimeSpan m_PreCpuTime;
        private TimeSpan m_CurCpuTime;

        private const long k_Kb = 1024;
        private const long k_Mb = 1024 * 1024;

        private class Styles
        {
            public GUIStyle font;
            public Styles()
            {
                font = new GUIStyle
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Normal,
                    richText = true,
                };

                font.normal.textColor = Color.green;
            }
        }

        private static Styles styles
        {
            get
            {
                if (s_Styles == null)
                    s_Styles = new Styles();

                return s_Styles;
            }
        }
        private static Styles s_Styles;

        public void SetEnable(bool enable)
        {
            enabled = enable;
        }

        private void Awake()
        {
            m_FpsDelay = 0.5F;
            m_CurCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
            m_PreCpuTime = m_CurCpuTime;
            m_CurTime = Time.realtimeSinceStartup;
            m_LastTime = m_CurTime;
        }

        private void OnGUI()
        {
            if (!enabled) return;

            GUILayout.Label(m_FpsText, styles.font);
            GUILayout.Label(m_HeapSizeText, styles.font);
            GUILayout.Label(m_UsedSizeText, styles.font);
            GUILayout.Label(m_AllocatedMemoryText, styles.font);
            GUILayout.Label(m_ReservedMemoryText, styles.font);
            GUILayout.Label(m_UnusedReservedMemoryText, styles.font);
            GUILayout.Label(m_CpuText, styles.font);
        }

        private void Update()
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            m_Index++;
            if (m_Index == m_IndexCount)
            {
                ShowProfilerMsg();
            }

            m_CurCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
            if ((m_CurCpuTime - m_PreCpuTime).TotalMilliseconds >= 1000)
            {
                ShowCpuMsg();
            }

            m_FramesIndex++;
            m_CurTime = Time.realtimeSinceStartup;
            if (m_CurTime - m_LastTime > m_FpsDelay)
            {
                ShowFpsMsg();
            }
        }

        private void ShowProfilerMsg()
        {
            m_Index = 0;
            m_HeapSizeText = "MonoHeap: " + Profiler.GetMonoHeapSizeLong() / k_Mb + " Mb";
            m_UsedSizeText = "MonoUsed: " + Profiler.GetMonoUsedSizeLong() / k_Mb + " Mb";
            m_AllocatedMemoryText = "TotalAllocatedMemory: " + Profiler.GetTotalAllocatedMemoryLong() / k_Mb + " Mb";
            m_ReservedMemoryText = "TotalReservedMemory: " + Profiler.GetTotalReservedMemoryLong() / k_Mb + " Mb";
            m_UnusedReservedMemoryText = "TotalUnusedReservedMemory: " + Profiler.GetTotalUnusedReservedMemoryLong() / k_Mb + " Mb";
        }

        private void ShowCpuMsg()
        {
            int interval = 1000;
            var value = (m_CurCpuTime - m_PreCpuTime).TotalMilliseconds / interval / Environment.ProcessorCount * 100;
            m_PreCpuTime = m_CurCpuTime;

            m_CpuText = "CpuUsage: " + value.ToString("f2");
        }

        private void ShowFpsMsg()
        {
            m_Fps = m_FramesIndex / (m_CurTime - m_LastTime);
            m_FramesIndex = 0;
            m_LastTime = m_CurTime;

            m_FpsText = "FPS: " + m_Fps.ToString("f2");
        }
    }
}