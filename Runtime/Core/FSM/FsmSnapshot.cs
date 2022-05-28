namespace Saro.Core
{
    public class FsmSnapshot<T>
    {
        public T CurrentState { get; private set; }
        public float StateAge { get; private set; }

        public FsmSnapshot(T currentState, float stateAge)
        {
            CurrentState = currentState;
            StateAge = stateAge;
        }
    }
}