namespace Saro.Core
{
    public abstract class AProcedureBase
    {
        public abstract void OnEnter();
        public abstract void OnUpdate<T>(FsmStateData<T> data);
        public abstract void OnLeave();
    }

}