using UnityEngine;

namespace Saro.Audio.Tests
{
    public class TestUnityAudio : MonoBehaviour
    {
        public AudioClip audioClip;

        private AudioPlayerHandle m_Handle;

        private void OnGUI()
        {
            if (GUILayout.Button("Play Audio"))
            {
                m_Handle = AudioManager.Current.PlaySE(audioClip);
            }

            if (GUILayout.Button("Stop Audio by handle"))
            {
                if (m_Handle)
                    m_Handle.AudioPlayer.StopAndRelease();
            }
        }
    }
}