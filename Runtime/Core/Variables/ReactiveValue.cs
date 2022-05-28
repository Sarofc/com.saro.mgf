using System;
using UnityEngine;

namespace Saro
{
    /// <summary>
    /// Raises an event whenever the value is set.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ReactiveValue<T>
    {
#if UNITY_EDITOR
        [SerializeField]
#endif
        private T m_Value;

        public event EventHandler<T> ValueChanged;

        public T Value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
                OnValueChanged(m_Value);
            }
        }

        public ReactiveValue()
        { }

        public ReactiveValue(T value)
        {
            m_Value = value;
        }

        protected virtual void OnValueChanged(T value)
        {
            ValueChanged?.Invoke(this, value);
        }

        public static implicit operator T(ReactiveValue<T> value)
        {
            return value.m_Value;
        }
    }

    [Serializable]
    public class IntReactiveValue : ReactiveValue<int> { }

    [Serializable]
    public class FloatReactiveValue : ReactiveValue<float> { }

    [Serializable]
    public class ByteReactiveValue : ReactiveValue<byte> { }

    [Serializable]
    public class LongReactiveValue : ReactiveValue<long> { }

    [Serializable]
    public class DoubleReactiveValue : ReactiveValue<double> { }

    [Serializable]
    public class BoolReactiveValue : ReactiveValue<bool> { }

    [Serializable]
    public class Vector3ReactiveValue : ReactiveValue<Vector3> { }

    [Serializable]
    public class Vector2ReactiveValue : ReactiveValue<Vector2> { }

    [Serializable]
    public class QuaternionReactiveValue : ReactiveValue<Quaternion> { }

    [Serializable]
    public class RectReactiveValue : ReactiveValue<Rect> { }

}