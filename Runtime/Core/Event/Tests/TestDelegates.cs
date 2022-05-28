using System;
using UnityEngine;

namespace Saro.Tests
{
    public class TestDelegates : MonoBehaviour
    {
        // Use this for initialization
        private void Start()
        {
            TestMyDelegates();

            UnityEngine.Profiling.Profiler.BeginSample("[delegate] CSharp");
            CSharpDelegate();
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("[delegate] MyDeletage");
            MyDelegates();
            UnityEngine.Profiling.Profiler.EndSample();
        }

        private void CSharpDelegate()
        {
            void Method1(int arg) { };
            void Method2(int arg) { };

            Action<int> action = Method1;

            //action += Method1;
            action += Method2;

            action += Method1;
            action += Method2;

            action += Method1;
            action += Method2;

            action += Method1;
            action += Method2;

            action += Method1;
            action += Method2;

            action?.Invoke(1);

            action -= Method1;

            action?.Invoke(2);

            action -= Method2;

            action?.Invoke(3);
        }

        private void MyDelegates()
        {
            void Method1(int arg) { };
            void Method2(int arg) { };

            var action = new FDelegates<int>();

            action += Method1;
            action += Method2;

            action += Method1;
            action += Method2;

            action += Method1;
            action += Method2;

            action += Method1;
            action += Method2;

            action += Method1;
            action += Method2;

            action.Invoke(1);

            action -= Method1;

            action.Invoke(2);

            action -= Method2;

            action.Invoke(3);
        }

        private void TestMyDelegates()
        {
            void Method1(int arg) => Debug.LogError("method1: " + arg);
            void Method2(int arg) => Debug.LogError("method2: " + arg);

            var action = new FDelegates<int>();

            action += Method1;
            action += Method2;
            action.Invoke(1);

            action -= Method1;
            action.Invoke(2);

            action -= Method2;
            action.Invoke(3);

            action += Method1;
            action += Method2;
            action.Invoke(4);
        }
    }
}