using System;
using NUnit.Framework;
using Saro.Events;
using UnityEngine;

namespace Saro.MgfTests
{

    public class TestEventPool
    {
        public abstract class TestEventArg : BaseEventArgs
        {
            public int callbackEventID;
        }

        public class TestEventArg1 : TestEventArg
        {
            public readonly static int s_ID = typeof(TestEventArg1).GetHashCode();
            public override int ID => s_ID;

            public override void IReferenceClear()
            {
                callbackEventID = 0;
            }
        }

        public class TestEventArg2 : TestEventArg
        {
            public readonly static int s_ID = typeof(TestEventArg2).GetHashCode();
            public override int ID => s_ID;

            public override void IReferenceClear()
            {

            }
        }

        [SetUp]
        public void Setup()
        {
        }

        public void TestMethod1(object sender, BaseEventArgs arg) { }
        public void TestMethod2(object sender, BaseEventArgs arg) {; }
        private void handleEvent(object sender, BaseEventArgs arg)
        {
            if (arg is TestEventArg _arg)
            {
                Assert.IsTrue(_arg.callbackEventID == arg.ID, "event failed");
            }
            else
            {
                Assert.Fail("not impl TestEventArg");
            }
        }

        [Test]
        public void Broadcast()
        {
            EventPool<BaseEventArgs> eventPool = new EventPool<BaseEventArgs>(EEventPoolMode.Default);
            eventPool.Subscribe(TestEventArg1.s_ID, TestMethod1);
            eventPool.Subscribe(TestEventArg2.s_ID, TestMethod2);

            var eventArgs = SharedPool.Rent<TestEventArg1>();
            eventArgs.callbackEventID = TestEventArg1.s_ID;
            eventPool.Broadcast(null, eventArgs);
        }

        [Test]
        public void DefaultMode_NoHandler()
        {
            EventPool<BaseEventArgs> eventPool = new EventPool<BaseEventArgs>(EEventPoolMode.Default);
            eventPool.Subscribe(0, TestMethod1);

            bool exception = false;

            try
            {
                var eventArgs = SharedPool.Rent<TestEventArg1>();
                eventArgs.callbackEventID = TestEventArg1.s_ID;
                eventPool.Broadcast(null, eventArgs);
            }
            catch (MyEventException)
            {
                exception = true;
            }

            if (!exception)
            {
                Assert.Fail("DefaultMode, 需要异常");
            }
        }

        [Test]
        public void DefaultMode_NoMultiHandler()
        {
            EventPool<BaseEventArgs> eventPool = new EventPool<BaseEventArgs>(EEventPoolMode.Default);

            bool exception = false;

            try
            {
                eventPool.Subscribe(TestEventArg1.s_ID, TestMethod1);
                eventPool.Subscribe(TestEventArg1.s_ID, TestMethod2);
                var eventArgs = SharedPool.Rent<TestEventArg1>();
                eventArgs.callbackEventID = TestEventArg1.s_ID;
                eventPool.Broadcast(null, eventArgs);
            }
            catch (MyEventException)
            {
                exception = true;
            }

            if (!exception)
            {
                Assert.Fail("DefaultMode, 需要异常");
            }
        }

        [Test]
        public void DefaultMode_NoDuplicateHandler()
        {
            EventPool<BaseEventArgs> eventPool = new EventPool<BaseEventArgs>(EEventPoolMode.Default);

            bool exception = false;

            try
            {
                eventPool.Subscribe(TestEventArg1.s_ID, TestMethod1);
                eventPool.Subscribe(TestEventArg1.s_ID, TestMethod1);

                var eventArgs = SharedPool.Rent<TestEventArg1>();
                eventArgs.callbackEventID = TestEventArg1.s_ID;
                eventPool.Broadcast(null, eventArgs);
            }
            catch (MyEventException)
            {
                exception = true;
            }

            if (!exception)
            {
                Assert.Fail("DefaultMode, 需要异常");
            }
        }

        [Test]
        public void AllowNoHandlerMode()
        {
            EventPool<BaseEventArgs> eventPool = new EventPool<BaseEventArgs>(EEventPoolMode.AllowNoHandler);
            eventPool.Subscribe(TestEventArg1.s_ID, TestMethod1);

            var eventArgs = SharedPool.Rent<TestEventArg1>();
            eventArgs.callbackEventID = TestEventArg1.s_ID;
            eventPool.Broadcast(null, eventArgs);
        }

        [Test]
        public void AllowMultiHandler()
        {
            EventPool<BaseEventArgs> eventPool = new EventPool<BaseEventArgs>(EEventPoolMode.AllowMultiHandler);
            eventPool.Subscribe(TestEventArg2.s_ID, TestMethod1);
            eventPool.Subscribe(TestEventArg2.s_ID, TestMethod2);

            var eventArgs = SharedPool.Rent<TestEventArg2>();
            eventArgs.callbackEventID = TestEventArg2.s_ID;
            eventPool.Broadcast(null, eventArgs);
        }

        [Test]
        public void AllowDuplicateHandler()
        {
            EventPool<BaseEventArgs> eventPool = new EventPool<BaseEventArgs>(EEventPoolMode.AllowDuplicateHandler);
            eventPool.Subscribe(TestEventArg1.s_ID, TestMethod1);
            eventPool.Subscribe(TestEventArg2.s_ID, TestMethod1);

            var eventArgs1 = SharedPool.Rent<TestEventArg1>();
            eventArgs1.callbackEventID = TestEventArg1.s_ID;
            eventPool.Broadcast(null, eventArgs1);

            var eventArgs2 = SharedPool.Rent<TestEventArg2>();
            eventArgs2.callbackEventID = TestEventArg2.s_ID;
            eventPool.Broadcast(null, eventArgs2);
        }
    }
}