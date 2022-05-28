using Saro;
using System;
using UnityEngine;

namespace Tetris.Assets.Scripts.Tests
{
    internal class TestDelegateSub : MonoBehaviour
    {
        private void Start()
        {
            CSharpDelegage();

            DoFDelegage();
        }

        private FDelegates action1 = new FDelegates();

        private void DoFDelegage()
        {
            action1 += Action11;
            action1 += Action12;
            action1 += Action13;

            action1?.Invoke();

            action1?.Invoke();
        }

        private void Action11()
        {
            Debug.LogError("TestDelegateSub action11 : 1");
        }

        private void Action12()
        {
            Debug.LogError("TestDelegateSub action12 : 2");

            action1 -= Action13;
        }

        private void Action13()
        {
            Debug.LogError("TestDelegateSub action13 : 3");
        }

        private Action action = null;
        private void CSharpDelegage()
        {
            action += Action1;
            action += Action2;
            action += Action3;

            action?.Invoke();

            action?.Invoke();
        }

        private void Action1()
        {
            Debug.LogError("TestDelegateSub action : 1");
        }

        private void Action2()
        {
            Debug.LogError("TestDelegateSub action : 2");

            action -= Action3;
        }

        private void Action3()
        {
            Debug.LogError("TestDelegateSub action : 3");
        }
    }
}
