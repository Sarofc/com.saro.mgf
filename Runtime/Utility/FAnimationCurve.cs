/*
 * https://blog.csdn.net/fucun1984686003/article/details/81086630
 */

#if FIXED_POINT_MATH
using Single = sfloat;
#else
using Single = System.Single;
#endif

using System;
using System.Runtime.Serialization;

namespace Saro.Utility
{
    [System.Serializable]
    public partial struct FKeyframe
    {
        /// <summary>
        ///  Describes the tangent when approaching this point from the previous point in the curve.
        /// </summary>
        public Single inTangent;

        /// <summary>
        /// The out tangent.
        /// </summary>
        public Single outTangent;

        /// <summary>
        ///  The time of the keyframe.
        /// </summary>
        public Single time;

        /// <summary>
        /// The value of the curve at keyframe.
        /// </summary>
        public Single value;

        public FKeyframe(Single inTangent, Single outTangent, Single time, Single value)
        {
            this.inTangent = inTangent;
            this.outTangent = outTangent;
            this.time = time;
            this.value = value;
        }
    }

    /// <summary>
    /// like UnityEngine.AnimationCurve
    /// </summary>
    [Serializable]
    public partial class FAnimationCurve
    {
        /// <summary>
        /// like UnityEngine.Keyframe
        /// </summary>
        //[HideInInspector]
        public FKeyframe[] keys;

        public FAnimationCurve()
        {
            keys = new FKeyframe[0];
        }

        public FAnimationCurve(FKeyframe[] keys)
        {
            this.keys = keys;
        }

        public Single Evaluate(Single x)
        {
            if (keys == null || keys.Length == 0)
            {
                return 0;
            }
            return Internal_Evaluate(keys, x);
        }

        // Hermite
        private Single Internal_Evaluate(FKeyframe[] keys, Single x)
        {
            var index = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (i == 0 && x < keys[i].time)
                {
                    return keys[0].value;
                }
                if (x <= keys[i].time)
                {
                    index = i;
                    if (i == 0)
                    {
                        index = 1;
                    }
                    break;
                }
            }
            if (index == 0)
            {
                return keys[keys.Length - 1].value;
            }
            var startIndex = index - 1;
            var endIndex = index;
            var t = x - keys[startIndex].time;
            var off_t = keys[startIndex].time;
            var off_p = keys[startIndex].value;
            var t0 = keys[startIndex].time - off_t;
            var t1 = keys[endIndex].time - off_t;

            var A = t1 - t0;
            var B = t1 * t1 - t0 * t0;
            var C = t1 * t1 * t1 - t0 * t0 * t0;
            var p0 = keys[startIndex].value - off_p;
            var p1 = keys[endIndex].value - off_p;
            var p0_d = keys[startIndex].outTangent;
            var p1_d = keys[endIndex].inTangent;

            var b4 = ((p1 - p0 - p0_d * A) / (B - 2 * A * t0 * t0) - (p1_d - p0_d) / (2 * A)) / ((C - 3 * A * t0 * t0) / (B - 2 * A * t0) - (3 * B / (2 * A)));
            var b3 = (p1_d - p0_d) / (2 * A) - 3 * B / (2 * A) * b4;
            var b2 = p0_d - (b3 * 2 * t0 + b4 * 3 * t0 * t0);
            var b1 = p0 - (b2 * t0 + b3 * t0 * t0 + b4 * t0 * t0 * t0);
            var pt = b1 + b2 * t + b3 * t * t + b4 * t * t * t;

            return pt + off_p;
        }

    }
}