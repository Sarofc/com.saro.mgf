using Cysharp.Threading.Tasks;
using Saro.Core;
using Saro.Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Saro.Audio
{
    /*
     * TODO
     *
     * 1. 对象池，生命周期，需要重构下，目前比较乱
     * 2. api、注释优化
     * 3. 要换成id接口？
     */

    /// <summary>
    /// 声音组件
    /// </summary>
    public sealed partial class AudioManager : IService
    {
        public static AudioManager Current => Main.Resolve<AudioManager>();

        private static int s_GlobalObjectID;

        /// <summary>
        /// AudioPlayer 对象池大小
        /// </summary>
        private const int k_AUDIO_SOURCE_POOL_MAX_SIZE = 36;

        public Transform ListenFollowTarget { get; set; }

        public Transform ListenDirectionTarget { get; set; }

        /// <summary>
        /// AudioPlayer 对象池
        /// </summary>
        private IObjectPool<AudioPlayer> m_AudioSourcePool;

        /// <summary>
        /// 保存激活AudioPlayer列表
        /// </summary>
        private List<AudioPlayer> m_ActivateAudioPlayers = new List<AudioPlayer>(64);
        private AudioMixer m_AudioMixer;
        public AudioMixer Mixer
        {
            get
            {
                return m_AudioMixer;
            }
        }

        public AudioMixerGroup EffectSoundGroup { get; private set; }

        public AudioMixerGroup BackgroundMusicGroup { get; private set; }

        public Vector3 ListenerPosition
        {
            get
            {
                if (m_AudioListenerGO == null)
                {
                    return Vector3.zero;
                }
                return m_AudioListenerGO.transform.position;
            }
        }

        private GameObject m_AudioManagerGO = null;
        private GameObject m_AudioListenerGO = null;

        /// <summary>
        /// BGM的AudioSource
        /// </summary>
        private class BGMPlayer
        {
            public AudioSource AudioSourceBGM { get; set; }
            public IAssetHandle AssetHandle { get; set; }

            public BGMPlayer(GameObject audioManagerGO, AudioMixerGroup backgroundMusicGroup)
            {
                var audioSourceBGM = new GameObject("AudioSourceBGM");
                audioSourceBGM.transform.parent = audioManagerGO.transform;
                AudioSourceBGM = audioSourceBGM.AddComponent<AudioSource>();
                AudioSourceBGM.outputAudioMixerGroup = backgroundMusicGroup;
            }
        }

        private BGMPlayer m_BGMPlayer;

        /// <summary>
        /// 初始化背景音樂的AudioSource
        /// </summary>
        private AudioSource m_AudioSourceBGM => m_BGMPlayer.AudioSourceBGM;

        /// <summary>
        /// 当前播放的BGM
        /// </summary>
        private AudioClip m_CurrentClipBGM;

        /// <summary>
        /// fade效果协程
        /// </summary>
        private IEnumerator m_FadeAudio;

        /// <summary>
        /// 是否正在处理停止bgm
        /// </summary>
        private bool m_IsStopingBGM;

        /// <summary>
        /// 初始化AudioManager
        /// </summary>
        private async UniTask InternalInitializeAsync()
        {
            if (m_AudioManagerGO != null)
                return;

            // TODO 工作流优化
            //  Editor 创建 AudioMixer，以及需要的文件夹，方便其他项目接入

            var handle = LoadAssetAsync("GameMixer.mixer", typeof(AudioMixer));
            handle.IncreaseRefCount();
            await handle;
            m_AudioMixer = handle.Asset as AudioMixer;
            if (m_AudioMixer == null)
            {
                return;
            }

            AudioMixerGroup[] findEffectGroup = m_AudioMixer.FindMatchingGroups("Master/SE");
            AudioMixerGroup[] findBGMGroup = m_AudioMixer.FindMatchingGroups("Master/BGM");

            if (findEffectGroup.Length != 0)
                EffectSoundGroup = findEffectGroup[0];
            if (findBGMGroup.Length != 0)
                BackgroundMusicGroup = findBGMGroup[0];

            m_AudioManagerGO = new GameObject($"[{nameof(AudioManager)}]");
            if (Application.isPlaying)
            {
                GameObject.DontDestroyOnLoad(m_AudioManagerGO);
            }
            m_AudioListenerGO = new GameObject("AudioListener");
            m_AudioListenerGO.AddComponent<AudioListener>();
            m_AudioListenerGO.transform.parent = m_AudioManagerGO.transform;

            m_BGMPlayer = new BGMPlayer(m_AudioManagerGO, BackgroundMusicGroup);
            InitSEAudioSourcePool();

            if (m_AudioMixer.GetFloat("SEVolume", out float seVolume))
                VolumeSE = LinearByDecibel(seVolume);

            if (m_AudioMixer.GetFloat("BGMVolume", out float bgmVolume))
                VolumeBGM = LinearByDecibel(bgmVolume);

            //Log.ERROR($"VolumeBGM: {VolumeBGM}   VolumeSE: {VolumeSE}");
        }

        void IService.Awake() { }

        void IService.Update()
        {
            if (m_AudioManagerGO == null) return;

            if (ListenFollowTarget != null)
            {
                m_AudioListenerGO.transform.position = ListenFollowTarget.position;

                if (ListenDirectionTarget != null)
                    m_AudioListenerGO.transform.rotation = ListenDirectionTarget.rotation;
            }
            else
            {
                // TODO 丢到初始化
                m_AudioListenerGO.transform.localPosition = Vector3.zero;
                m_AudioListenerGO.transform.localRotation = Quaternion.identity;
            }

            if (m_CurFrameSEClip.Count > 0)
            {
                m_CurFrameSEClip.Clear();
            }
        }

        void IService.Dispose() { }

        #region 对象池

        private void InitSEAudioSourcePool()
        {
            m_AudioSourcePool = new ObjectPool<AudioPlayer>(OnCreateAudioPlayerInstance, OnGetAudioPlayerInstance, null, OnDestroyAudioPlayerInstance, true, k_AUDIO_SOURCE_POOL_MAX_SIZE, k_AUDIO_SOURCE_POOL_MAX_SIZE);

            // TODO 预加载一部分audioplayer
        }

        private AudioPlayer OnCreateAudioPlayerInstance()
        {
            var gameObject = new GameObject();
            if (m_AudioManagerGO != null)
                gameObject.transform.parent = m_AudioManagerGO.transform;
            var audioPlayer = gameObject.AddComponent<AudioPlayer>();
            return audioPlayer;
        }

        private void OnGetAudioPlayerInstance(AudioPlayer audioPlayer)
        {
#if UNITY_EDITOR
            audioPlayer.name = "AudioPlayer-" + s_GlobalObjectID;
#endif
            audioPlayer.ObjectID = /*s_GlobalObjectID == uint.MaxValue ? 1 :*/ ++s_GlobalObjectID;
        }

        private void OnDestroyAudioPlayerInstance(AudioPlayer audioPlayer)
        {
            if (audioPlayer != null)
                GameObject.Destroy(audioPlayer.gameObject);
        }

        #endregion

        #region 音乐

        /// <summary>
        /// 背景音乐音量
        /// </summary>
        private float m_VolumeBGM = 0.5f;
        public float VolumeBGM
        {
            get
            {
                return m_VolumeBGM;
            }
            set
            {
                if (m_AudioSourceBGM == null)
                {
                    return;
                }
                m_VolumeBGM = value;
                FadeMixer("BGMVolume", DecibelByLinear(m_VolumeBGM), 0.5f);
            }
        }

        /// <summary>
        /// 是否正在停止或已经停止播放背景音乐
        /// </summary>
        /// <returns></returns>
        public bool IsStopedOrStopingBGM()
        {
            if (!m_AudioSourceBGM.isPlaying)
            {
                return true;
            }

            return m_IsStopingBGM;
        }

        public async UniTask PlayBGMAsync(string assetName, float time = 1f, Action finishCallback = null, bool oneShot = false, bool forceReplay = false)
        {
            var assetHandle = LoadAssetAsync(assetName, typeof(AudioClip));
            await assetHandle;
            AudioClip clip = assetHandle.GetAsset<AudioClip>();

            PlayBGMInternal(clip, time, finishCallback, oneShot, forceReplay, assetHandle);
        }

        public void PlayBGM(string assetName, float time = 1f, Action finishCallback = null, bool oneShot = false, bool forceReplay = false)
        {
            var assetHandle = LoadAsset(assetName, typeof(AudioClip));
            AudioClip clip = assetHandle.GetAsset<AudioClip>();

            PlayBGMInternal(clip, time, finishCallback, oneShot, forceReplay, assetHandle);
        }

        public void PlayBGM(AudioClip clip, float time = 1f, Action finishCallback = null, bool oneShot = false, bool forceReplay = false)
        {
            PlayBGMInternal(clip, time, finishCallback, oneShot, forceReplay);
        }

        private void PlayBGMInternal(AudioClip clip, float time = 1f, Action finishCallback = null, bool oneShot = false, bool forceReplay = false, IAssetHandle assetHandle = null)
        {
            if (clip == null)
                return;

            if (forceReplay == false && m_AudioSourceBGM.isPlaying == true && m_CurrentClipBGM == clip)
                return; // 忽略同一clip

            m_IsStopingBGM = false;
            m_CurrentClipBGM = clip;

            void OnFadout()
            {
                if (m_BGMPlayer.AssetHandle != null)
                    m_BGMPlayer.AssetHandle.DecreaseRefCount();
                m_BGMPlayer.AssetHandle = assetHandle;

                m_AudioSourceBGM.clip = m_CurrentClipBGM;
                m_AudioSourceBGM.loop = oneShot == false;
                m_AudioSourceBGM.Play();

                FadeMixer("BGMVolume", DecibelByLinear(m_VolumeBGM), time, finishCallback);
            }

            StopBGMInternal(time, OnFadout);
        }

        public void StopBGM(float time = 1f, Action finishCallback = null)
        {
            m_IsStopingBGM = true;

            StopBGMInternal(time, finishCallback);
        }

        private void StopBGMInternal(float time, Action finishCallback)
        {
            void OnFinish()
            {
                m_FadeAudio = null;
                m_AudioSourceBGM.Stop();
                m_AudioSourceBGM.clip = null;
                m_IsStopingBGM = false;
                finishCallback?.Invoke();
            }

            if (m_AudioSourceBGM.isPlaying == false)
                OnFinish();
            else
                FadeMixer("BGMVolume", -80f, time, OnFinish);
        }

        public void MuteBGM(bool mute)
        {
            m_AudioSourceBGM.mute = mute;
        }

        private float DecibelByLinear(float linear)
        {
            if (linear <= 0.001f) return -80f;
            return Mathf.Log10(linear) * 20f;
        }
        private float LinearByDecibel(float decibel)
        {
            return Mathf.Pow(10f, decibel / 20f);
        }

        private void FadeMixer(string paramName, float value, float time, Action finishCallback = null)
        {
            if (m_FadeAudio != null)
            {
                Main.CancelCoroutine(m_FadeAudio);
            }

            m_FadeAudio = FadeMixerParam(paramName, value, time, finishCallback);
            Main.RunCoroutine(m_FadeAudio);
        }

        private IEnumerator FadeMixerParam(string paramName, float value, float time, Action finishCallback = null)
        {
            if (m_AudioMixer.GetFloat(paramName, out float originValue))
            {
                var deltaTime = 1f / 30f;
                for (float _time = 0; _time < time; _time += deltaTime)
                {
                    m_AudioMixer.SetFloat(paramName, Mathf.Lerp(originValue, value, _time / time));
                    yield return null;
                }
                m_AudioMixer.SetFloat(paramName, value);
            }

            finishCallback?.Invoke();

            m_FadeAudio = null;
        }

        private IEnumerator FadeoutAudio(AudioSource audioSource, float speed = 1, Action finishCallback = null)
        {
            if (audioSource != null)
            {
                var _OldVolume = m_AudioSourceBGM.volume;
                for (float _Time = 0; _Time < 1; _Time += Time.deltaTime * speed)
                {
                    audioSource.volume = Mathf.Lerp(_OldVolume, 0, _Time);
                    yield return null;
                }
                audioSource.volume = 0;
            }
            finishCallback?.Invoke();
        }

        #endregion

        #region 音效

        /// <summary>
        /// 音效音量
        /// </summary>
        public float VolumeSE { get; set; }
        /// <summary>
        /// 音效静音
        /// </summary>
        private bool IsMuteSE { get; set; }
        /// <summary>
        /// 当前帧在播的音效
        /// </summary>
        private List<string> m_CurFrameSEClip = new List<string>();

        public async UniTask<ObjectHandle<AudioPlayer>> PlaySEAsync(string assetName, float volumeScale = 1, bool loop = false, GameObject target = null)
        {
            if (IsMuteSE || VolumeSE <= 0)
            {
                return default;
            }

            //当前帧已经在播放这个音效了，不需要再播放
            if (m_CurFrameSEClip.Contains(assetName))
            {
                return default;
            }
            var assetHandle = LoadAssetAsync(assetName, typeof(AudioClip));
            await assetHandle;
            var clip = assetHandle.GetAsset<AudioClip>();

            if (clip == null)
            {
                return default;
            }

            //添加到当前帧在播的音效
            m_CurFrameSEClip.Add(assetName);

            return PlaySEInternal(clip, volumeScale, loop, target, assetHandle);
        }

        public ObjectHandle<AudioPlayer> PlaySE(string assetName, float volumeScale = 1, bool loop = false, GameObject target = null)
        {
            //禁音时直接返回
            if (IsMuteSE || VolumeSE <= 0)
            {
                return default;
            }

            //当前帧已经在播放这个音效了，不需要再播放
            if (m_CurFrameSEClip.Contains(assetName))
            {
                return default;
            }

            var assetHandle = LoadAsset(assetName, typeof(AudioClip));
            var clip = assetHandle.GetAsset<AudioClip>();
            if (clip == null)
            {
                return default;
            }

            //添加到当前帧在播的音效
            m_CurFrameSEClip.Add(assetName);

            return PlaySEInternal(clip, volumeScale, loop, target, assetHandle);
        }

        public ObjectHandle<AudioPlayer> PlaySE(AudioClip clip, float volumeScale = 1, bool loop = false, GameObject target = null)
        {
            return PlaySEInternal(clip, volumeScale, loop, target, null);
        }

        private ObjectHandle<AudioPlayer> PlaySEInternal(AudioClip clip, float volumeScale = 1, bool loop = false, GameObject target = null, IAssetHandle assetHandle = null)
        {
            if (m_AudioSourcePool == null)
            {
                return default;
            }

            if (clip == null)
                return default;

            var audioPlayer = m_AudioSourcePool.Rent();
            audioPlayer.Clip = clip;
            audioPlayer.Volume = VolumeSE * volumeScale;
            audioPlayer.Loop = loop;
            audioPlayer.FollowTarget = target;
            audioPlayer.Sound3D = target != null;
            audioPlayer.Mute = IsMuteSE;
            audioPlayer.Play();

            Debug.Assert(audioPlayer.AssetHandle == null, "audioPlayer.AssetHandle may not DecreaseRefCount at release.");
            audioPlayer.AssetHandle = assetHandle;

            m_ActivateAudioPlayers.Add(audioPlayer);

            return new(audioPlayer);
        }

        private ObjectHandle<AudioPlayer> PlaySEInternal(AudioClip clip, Vector3 target, float volumeScale = 1, bool loop = false, IAssetHandle assetHandle = null)
        {
            if (m_AudioSourcePool == null)
            {
                return default;
            }

            if (clip == null)
                return default;

            var audioPlayer = m_AudioSourcePool.Rent();
            audioPlayer.Clip = clip;
            audioPlayer.Volume = VolumeSE * volumeScale;
            audioPlayer.Loop = loop;
            audioPlayer.FollowTargetV3 = target;
            audioPlayer.Sound3D = target != Vector3.zero;
            audioPlayer.Mute = IsMuteSE;
            audioPlayer.Play();

            m_ActivateAudioPlayers.Add(audioPlayer);

            return new(audioPlayer);
        }

        public void StopAllSE()
        {
            if (m_AudioSourcePool == null)
            {
                return;
            }

            var audioPlayers = m_ActivateAudioPlayers;

            if (audioPlayers == null)
                return;

            for (int i = 0; i < audioPlayers.Count; ++i)
            {
                var player = audioPlayers[i];
                Release(player);
            }
        }

        internal void Release(AudioPlayer audioPlayer)
        {
            if (audioPlayer != null)
            {
                if (!audioPlayer.gameObject.activeSelf) return;

                audioPlayer.Stop();
                audioPlayer.Clip = null;
                audioPlayer.FollowTarget = null;

                audioPlayer.ObjectID = 0;

                if (audioPlayer.AssetHandle != null)
                {
                    audioPlayer.AssetHandle.DecreaseRefCount();
                    audioPlayer.AssetHandle = null;
                }

                audioPlayer.gameObject.SetActive(false);

                if (m_AudioSourcePool != null)
                {
                    m_AudioSourcePool.Return(audioPlayer);
                    m_ActivateAudioPlayers.Remove(audioPlayer);
                }
            }
        }

        public void MuteSE(bool mute)
        {
            if (m_AudioSourcePool == null)
            {
                return;
            }

            if (mute != IsMuteSE)
            {
                var audioPlayers = m_ActivateAudioPlayers;
                if (audioPlayers == null)
                    return;

                for (int i = 0; i < audioPlayers.Count; ++i)
                {
                    var player = audioPlayers[i];
                    player.Mute = mute;
                }
                IsMuteSE = mute;
            }
        }

        #endregion
    }

    public partial class AudioManager
    {
        public string AudioAssetPath { get; private set; }

        public async UniTask InitializeAsync(string audioAssetPath)
        {
            AudioAssetPath = audioAssetPath;

            await InternalInitializeAsync();
        }

        public IAssetHandle LoadAsset(string assetPath, Type type)
        {
            return IAssetManager.Current.LoadAsset(AudioAssetPath + assetPath, type);
        }

        public IAssetHandle LoadAssetAsync(string assetPath, Type type)
        {
            return IAssetManager.Current.LoadAssetAsync(AudioAssetPath + assetPath, type);
        }
    }
}