#if UNITY_2017_1_OR_NEWER

#if FIXED_POINT_MATH
using Single = Saro.FPMath.sfloat;
#else
using Single = System.Single;
#endif

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Saro.Utility
{
    partial struct FKeyframe
    {
        public static implicit operator FKeyframe(Keyframe uKeyframe)
        {
            FKeyframe sKeyframe = new FKeyframe
            {
                time = (Single)uKeyframe.time,
                value = (Single)uKeyframe.value,
                inTangent = (Single)uKeyframe.inTangent,
                outTangent = (Single)uKeyframe.outTangent
            };
            return sKeyframe;
        }

        public static implicit operator Keyframe(FKeyframe sKeyframe)
        {
            var uKeyframe = new Keyframe
            {
                time = (float)sKeyframe.time,
                value = (float)sKeyframe.value,
                inTangent = (float)sKeyframe.inTangent,
                outTangent = (float)sKeyframe.outTangent
            };
            return uKeyframe;
        }
    }

    partial class FAnimationCurve 
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif
    {
        public static implicit operator FAnimationCurve(AnimationCurve uAnimationCurve)
        {
            if (uAnimationCurve == null) return null;
            var fAnimationCurve = new FAnimationCurve
            {
                keys = new FKeyframe[uAnimationCurve.keys.Length]
            };
            for (int i = 0; i < uAnimationCurve.keys.Length; i++)
            {
                fAnimationCurve.keys[i] = uAnimationCurve.keys[i];
            }
#if UNITY_EDITOR
            fAnimationCurve.m_Curve_Editor = uAnimationCurve;
#endif
            return fAnimationCurve;
        }

        public static implicit operator AnimationCurve(FAnimationCurve fAnimationCurve)
        {
            if (fAnimationCurve == null) return null;
            var uAnimationCurve = new AnimationCurve();
            for (int i = 0; i < fAnimationCurve.keys.Length; i++)
            {
                uAnimationCurve.AddKey(fAnimationCurve.keys[i]);
            }
            return uAnimationCurve;
        }

#if UNITY_EDITOR
        [SerializeField]
        private AnimationCurve m_Curve_Editor;

        [OnDeserialized]
        private void AfterDeserialization(StreamingContext ctx)
        {
            m_Curve_Editor = this;
        }

        [OnSerializing]
        private void BeforeSerialization(StreamingContext ctx)
        {
            if (m_Curve_Editor != null)
            {
                keys = new FKeyframe[m_Curve_Editor.keys.Length];

                for (int i = 0; i < m_Curve_Editor.keys.Length; i++)
                {
                    keys[i] = m_Curve_Editor.keys[i];
                }
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            BeforeSerialization(default);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            AfterDeserialization(default);
        }
#endif
    }
}

#endif