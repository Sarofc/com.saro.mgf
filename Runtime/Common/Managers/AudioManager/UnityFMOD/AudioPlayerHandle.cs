//using System;

//namespace Saro.Audio
//{
//    public struct AudioPlayerHandle : IEquatable<AudioPlayerHandle>
//    {
//        public AudioPlayer AudioPlayer { get; private set; }

//        public int Handle => m_CachedObjectID;
//        private readonly int m_CachedObjectID;

//        public AudioPlayerHandle(AudioPlayer audioPlayer)
//        {
//            AudioPlayer = audioPlayer;
//            m_CachedObjectID = audioPlayer.ObjectID;
//        }

//        private bool IsValid()
//        {
//            return AudioPlayer != null && m_CachedObjectID == AudioPlayer.ObjectID;
//        }

//        public static implicit operator bool(AudioPlayerHandle handle)
//        {
//            return handle.IsValid();
//        }

//        public static explicit operator AudioPlayer(AudioPlayerHandle handle)
//        {
//            return handle ? handle.AudioPlayer : null;
//        }

//        public bool Equals(AudioPlayerHandle other)
//        {
//            return CompareObjects(this, other);
//        }

//        public override bool Equals(object obj)
//        {
//            if (!(obj is AudioPlayerHandle)) return false;

//            return Equals((AudioPlayerHandle)obj);
//        }

//        public static bool operator ==(AudioPlayerHandle lhs, AudioPlayerHandle rhs)
//        {
//            return CompareObjects(lhs, rhs);
//        }

//        public static bool operator !=(AudioPlayerHandle lhs, AudioPlayerHandle rhs)
//        {
//            return !CompareObjects(lhs, rhs);
//        }

//        private static bool CompareObjects(AudioPlayerHandle lhs, AudioPlayerHandle rhs)
//        {
//            var validA = lhs.IsValid();
//            var validB = rhs.IsValid();

//            if (validA && !validB) return false;
//            if (!validA && validB) return false;

//            return lhs.m_CachedObjectID == rhs.m_CachedObjectID;
//        }

//        public override int GetHashCode()
//        {
//            return m_CachedObjectID;
//        }
//    }
//}
