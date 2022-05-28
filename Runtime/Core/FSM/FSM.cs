using System;
using System.Collections.Generic;

namespace Saro.Core
{
    /*
     *  Fork https://github.com/xanathar/FSMsharp
     */
    public sealed class FSM<T>
    {

        public Action<T, T> OnStateChanged;

        public T CurrentState
        {
            get { return m_CurrentState; }
            set
            {
                m_CurrentStateBehaviour?.DoLeave();
                m_StateAge = -1f;

                PreviousState = m_CurrentState;

                m_CurrentStateBehaviour = m_Behaviours[value];
                m_CurrentState = value;
                m_Data.Behaviour = m_CurrentStateBehaviour;
                m_Data.State = m_CurrentState;

                m_CurrentStateBehaviour?.DoEnter();

                OnStateChanged?.Invoke(PreviousState, m_CurrentState);
            }
        }

        public T PreviousState { get; private set; }

        public IReadOnlyDictionary<T, FsmStateBehaviour<T>> Behaviors => m_Behaviours;
        public FsmStateData<T> Data => m_Data;

        private readonly string m_FsmName;
        private readonly FsmStateData<T> m_Data;
        private readonly Dictionary<T, FsmStateBehaviour<T>> m_Behaviours = new Dictionary<T, FsmStateBehaviour<T>>();

        private FsmStateBehaviour<T> m_CurrentStateBehaviour;

        private float m_StateAge;
        private float m_AbsoluteTime;
        private float m_StateTime;
        private float m_NormalizedTime;

        private T m_CurrentState;

        public FSM(string fsmName) : this(fsmName, null) { }

        public FSM(string fsmName, object userData)
        {
            m_FsmName = fsmName;

            m_StateAge = -1f;
            m_NormalizedTime = 0f;

            m_Data = new FsmStateData<T>();
            m_Data.Fsm = this;
            m_Data.UserData = userData;
        }

        public FsmStateBehaviour<T> Add(T state)
        {
            FsmStateBehaviour<T> behaviour = new FsmStateBehaviour<T>(state);
            m_Behaviours.Add(state, behaviour);
            return behaviour;
        }

        public void DoUpdate(float elapsedTime)
        {
            if (m_StateAge < 0f) m_StateAge = elapsedTime;

            m_AbsoluteTime = elapsedTime;
            m_StateTime = (m_AbsoluteTime - m_StateAge);

            if (m_CurrentStateBehaviour == null)
            {
                throw new NullReferenceException(string.Format("[FSM {0}] : current state is null", m_FsmName));
            }

            if (m_CurrentStateBehaviour.Duration.HasValue)
            {
                // clamp01
                m_NormalizedTime = Math.Max(0f, Math.Min(1f, m_StateTime / m_CurrentStateBehaviour.Duration.Value));
            }

            m_Data.StateTime = m_StateTime;
            m_Data.AbsoluteTime = m_AbsoluteTime;
            m_Data.NormalizedTime = m_NormalizedTime;

            // some transition conditions will be called in update
            m_CurrentStateBehaviour.DoUpdate(m_Data);

            // timer transition check
            if (m_NormalizedTime >= 1f && m_CurrentStateBehaviour.NextStateSelector != null)
            {
                CurrentState = m_CurrentStateBehaviour.NextStateSelector();
                m_StateAge = elapsedTime;
                m_NormalizedTime = 0f;
            }
        }


        public void DoLateUpdate()
        {
            m_CurrentStateBehaviour.DoLateUpdate();
        }

        public void DoFixedUpdate()
        {
            m_CurrentStateBehaviour.DoFixedUpdate();
        }

        public FsmSnapshot<T> SaveSnapshot()
        {
            return new FsmSnapshot<T>(m_CurrentState, m_StateAge);
        }

        public void RestoreSnapshot(FsmSnapshot<T> snapshot)
        {
            CurrentState = snapshot.CurrentState;
            m_StateAge = snapshot.StateAge;
        }

        public bool ContainsState(T state)
        {
            return m_Behaviours.ContainsKey(state);
        }
    }
}