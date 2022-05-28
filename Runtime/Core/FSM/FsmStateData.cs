namespace Saro.Core
{
    public class FsmStateData<T>
    {
        public FSM<T> Fsm { get; set; }
        public FsmStateBehaviour<T> Behaviour { get; set; }
        public T State { get; set; }
        public float StateTime { get; set; }
        public float AbsoluteTime { get; set; }
        public float NormalizedTime { get; set; }
        public object UserData { get; set; }
    }
}