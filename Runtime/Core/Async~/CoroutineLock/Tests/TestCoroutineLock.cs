#if UNITY_EDITOR

using Saro;
using Saro.Tasks;

namespace Saro.Tests
{
    public class TestCoroutineLock : UnityEngine.MonoBehaviour
    {
        private void Start()
        {
            DoTests();
        }

        private void DoTests()
        {
            WaitTimes(2000).Coroutine();

            WaitTimes(500).Coroutine();

            WaitTimes(800).Coroutine();
        }

        private async FTask WaitTimes(long time)
        {
            var coroutineLockComponent = FGame.Resolve<CoroutineLockComponent>();
            using (await coroutineLockComponent.Wait(0, 1, 1000))
            {
                await FGame.Resolve<TimerComponent>().WaitAsync(time);
            }

            Log.ERROR("timeout: " + time);
        }
    }
}

#endif