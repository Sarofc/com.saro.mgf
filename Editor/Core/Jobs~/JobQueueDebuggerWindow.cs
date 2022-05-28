using UnityEngine;
using UnityEditor;

namespace Saro
{
    public class JobQueueDebuggerWindow : EditorWindow
    {
        [MenuItem("Tools/Debug/JobQueue Debugger")]
        static void ShowWindow()
        {
            var window = GetWindow<JobQueueDebuggerWindow>();
            window.Show();
        }

        private void OnGUI()
        {
            if (Application.isPlaying == false)
            {
                EditorGUILayout.HelpBox("Appllication is not currently running.", MessageType.Info);
                return;
            }

            if (Processor.JobQueue == null)
            {
                return;
            }

            GUILayout.Label("Processor Count: " + SystemInfo.processorCount);
            EditorGUILayout.Space();
            GUILayout.Label("WorkerThread Count: " + Processor.JobQueue.WorkerThreadCount);
            GUILayout.Label("InProcessing: " + Processor.JobQueue.InProcessing);
            GUILayout.Label("Job Queued Count: " + Processor.JobQueue.JobQueuedCount);
            GUILayout.Label("Job Finished Count: " + Processor.JobQueue.JobFinishedCount);

            Repaint();
        }
    }
}