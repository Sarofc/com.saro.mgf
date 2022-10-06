using Saro.Core;
using Saro.Pool;
using System;
using UnityEngine;

namespace Saro.Audio
{
    [ExecuteInEditMode]
    public sealed class AudioPlayer : MonoBehaviour, IHandledObject
    {
        public int ObjectID { get; internal set; }

        /// <summary>
        /// 资源handle，用于管理引用
        /// </summary>
        public IAssetHandle AssetHandle { get; internal set; }

        /// <summary>
        /// 播放完成回调
        /// </summary>
        public Action playerEnd;

        public AudioClip Clip
        {
            get { return m_AudioSource.clip; }
            set { m_AudioSource.clip = value; }
        }

        public bool PlayOnAwake
        {
            get { return m_AudioSource.playOnAwake; }
            set { m_AudioSource.playOnAwake = value; }
        }

        public bool Loop
        {
            get { return m_AudioSource.loop; }
            set { m_AudioSource.loop = value; }
        }

        public float Volume
        {
            get { return m_AudioSource.volume; }
            set { m_AudioSource.volume = value; }
        }

        public bool Mute
        {
            get { return m_AudioSource.mute; }
            set { m_AudioSource.mute = value; }
        }

        public bool Sound3D
        {
            get { return m_AudioSource.spatialBlend != 0; }
            set { m_AudioSource.spatialBlend = value ? 1 : 0; }
        }

        /// <summary>
        /// 3D音效要追踪目标
        /// </summary>
        public GameObject FollowTarget
        {
            get
            {
                return m_FollowTarget;
            }
            set
            {
                m_FollowTarget = value;
                m_HasFollowTarget = (m_FollowTargetV3 != Vector3.zero) || (m_FollowTarget != null);
            }
        }

        /// <summary>
        /// 3D音效要追踪目标
        /// </summary>
        public Vector3 FollowTargetV3
        {
            get
            {
                return m_FollowTargetV3;
            }
            set
            {
                m_FollowTargetV3 = value;
                m_HasFollowTarget = (m_FollowTargetV3 != Vector3.zero) || (m_FollowTarget != null);
            }
        }

        private AudioSource m_AudioSource;
        private float m_OriginVolume;
        private float m_FadeVolumeValue;
        private float m_FadeVolumeTime;
        private float m_FadedVolumeTime;
        private Action<AudioPlayer> m_OnFadeFinish;
        private GameObject m_FollowTarget;
        private Vector3 m_FollowTargetV3;
        private bool m_HasFollowTarget;

        private void Awake()
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
            m_AudioSource.outputAudioMixerGroup = AudioManager.Current.EffectSoundGroup;
            m_AudioSource.dopplerLevel = 0;
            m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
            m_AudioSource.minDistance = 5;
            m_AudioSource.maxDistance = 30;
            gameObject.SetActive(false);
        }

        public void Play()
        {
            Stop();
            gameObject.SetActive(true);
            UpdateFollowPosition();
            m_AudioSource.Play();
        }

        public void Stop()
        {
            m_OnFadeFinish = null;
            m_AudioSource.Stop();
        }

        public void StopAndRelease()
        {
            Stop();
            AudioManager.Current.Release(this);
        }

        public void FadeOutThenRelease()
        {
            FadeVolume(0, 0.5f, (_player) =>
            {
                _player.Stop();
                AudioManager.Current.Release(_player);
            });
        }

        public void FadeVolume(float value, float time, Action<AudioPlayer> onFinish = null)
        {
            if (time <= 0f)
            {
                Volume = value * AudioManager.Current.VolumeSE;
                m_OriginVolume = value;
                m_FadeVolumeValue = value;
                m_FadedVolumeTime = 0;
                m_FadeVolumeTime = 0;
                onFinish?.Invoke(this);
            }
            else
            {
                m_OriginVolume = Volume;
                m_FadeVolumeValue = Mathf.Clamp01(value * AudioManager.Current.VolumeSE);
                m_FadedVolumeTime = 0;
                m_FadeVolumeTime = time;
                m_OnFadeFinish = onFinish;
            }
        }

        private void Update()
        {
            UpdateFollowPosition();

            if (m_AudioSource.isPlaying == false)
            {
                AudioManager.Current.Release(this);
                if (playerEnd != null)
                    playerEnd();
            }

            if (m_FadedVolumeTime < m_FadeVolumeTime)
            {
                m_FadedVolumeTime += Time.deltaTime;
                float progress = Mathf.Clamp01(m_FadedVolumeTime / m_FadeVolumeTime);
                Volume = (m_FadeVolumeValue - m_OriginVolume) * progress + m_OriginVolume;

                if (progress >= 1f)
                {
                    m_OnFadeFinish?.Invoke(this);
                    m_OnFadeFinish = null;
                }
            }
        }

        /// <summary>
        /// 不直接Attach到目标的子物件是怕目标被销毁，AudioPlayer也被销毁
        /// </summary>
        private void UpdateFollowPosition()
        {
            if (m_HasFollowTarget)
            {
                if (FollowTarget != null)
                {
                    transform.position = FollowTarget.transform.position;
                }
                else if (FollowTargetV3 != Vector3.zero)
                {
                    transform.position = FollowTargetV3;
                }
                float _Distance = Vector3.Distance(AudioManager.Current.ListenerPosition, transform.position);
                m_AudioSource.spatialBlend = SpatialBlendByDistance(_Distance, 2.5f, 10f);
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }
        }

        private float SpatialBlendByDistance(float distance, float twoDHoldDistance, float threeDHoldDistance)
        {
            if (distance < twoDHoldDistance)
                return 0f;
            else if (distance > threeDHoldDistance)
                return 1f;

            return Mathf.Clamp01((distance - twoDHoldDistance) / (threeDHoldDistance - twoDHoldDistance));
        }
    }
}
