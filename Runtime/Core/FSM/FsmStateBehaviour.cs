using System;

namespace Saro.Core
{
    public sealed class FsmStateBehaviour<T>
    {

        /// <summary>
        /// state duraion
        /// if duration is null, this state is looped
        /// </summary>
        /// <value></value>
        public float? Duration { get; private set; }

        /// <summary>
        /// The next state
        /// </summary>
        /// <value></value>
        public Func<T> NextStateSelector { get; private set; }

        public T State { get; private set; }

        private Action m_EnterCallbacks;
        private Action m_ExitCallbacks;
        private Action<FsmStateData<T>> m_UpdateCallbacks;
        private Action m_LateUpdateCallbacks;
        private Action m_FixedUpdateCallbacks;

        public FsmStateBehaviour(T state)
        {
            State = state;
        }

        /// <summary>
        /// be called when FSM the enters this state
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public FsmStateBehaviour<T> OnEnter(Action callback)
        {
            m_EnterCallbacks += callback;
            return this;
        }

        /// <summary>
        /// be called when FSM the exits this state
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public FsmStateBehaviour<T> OnLeave(Action callback)
        {
            m_ExitCallbacks += callback;
            return this;
        }

        /// <summary>
        /// be called every frame when this state is actived
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public FsmStateBehaviour<T> OnUpdate(Action<FsmStateData<T>> callback)
        {
            m_UpdateCallbacks += callback;
            return this;
        }

        /// <summary>
        /// be called every frame when this state is actived
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public FsmStateBehaviour<T> OnLateUpdate(Action callback)
        {
            m_LateUpdateCallbacks += callback;
            return this;
        }

        /// <summary>
        /// be called every frame when this state is actived
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public FsmStateBehaviour<T> OnFixedUpdate(Action callback)
        {
            m_FixedUpdateCallbacks += callback;
            return this;
        }

        /// <summary>
        /// time condition, transition to next state after the given time
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public FsmStateBehaviour<T> Expires(float duration, T state)
        {
            Duration = duration;
            NextStateSelector = () => state;
            return this;
        }

        /// <summary>
        /// user condition, like input event etc.
        /// the index lower, the priority higher
        /// </summary>
        /// <param name="condition">conditon of transition</param>
        /// <param name="state">destnation state</param>
        /// <returns></returns>
        //public FsmStateBehaviour<T> Condition(Func<bool> condition, T state)
        //{
        //    Transitions.Add(new Transition(condition, state));
        //    return this;
        //}

        internal void DoUpdate(FsmStateData<T> data)
        {
            m_UpdateCallbacks?.Invoke(data);
        }

        internal void DoLateUpdate()
        {
            m_LateUpdateCallbacks?.Invoke();
        }

        internal void DoFixedUpdate()
        {
            m_FixedUpdateCallbacks?.Invoke();
        }

        internal void DoEnter()
        {
            m_EnterCallbacks?.Invoke();
        }

        internal void DoLeave()
        {
            m_ExitCallbacks?.Invoke();
        }
    }
}