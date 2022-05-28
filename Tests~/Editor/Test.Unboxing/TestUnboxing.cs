using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Saro.MgfTests
{
    public class TestUnboxing
    {
        MyStruct myStruct = new MyStruct();
        private object obj;

        [SetUp]
        public void Setup()
        {
            obj = myStruct;
        }

        [Test, Performance]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public void Test_Unboxing(int runCount)
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < runCount; i++)
                {
                    var res = Unboxing(obj);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(15)
            .IterationsPerMeasurement(5)
            //.GC()
            .Run();
        }

        [Test, Performance]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public void Test_Generic(int runCount)
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < runCount; i++)
                {
                    var res = Generic<MyStruct>(myStruct);
                }
            })
            .WarmupCount(5)
            .MeasurementCount(15)
            .IterationsPerMeasurement(5)
            //.GC()
            .Run();
        }

        [Test, Performance]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public void Test_GenericRef(int runCount)
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < runCount; i++)
                {
                    var res = Generic<MyStruct>(in myStruct);
                }
            })
           .WarmupCount(5)
           .MeasurementCount(15)
           .IterationsPerMeasurement(5)
           //.GC()
           .Run();
        }

        [Test, Performance]
        [TestCase(1000)]
        [TestCase(10000)]
        [TestCase(100000)]
        public void Test_GenericRefReturn(int runCount)
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < runCount; i++)
                {
                    ref var res = ref GenericRefReturn<MyStruct>(ref myStruct);
                }
            })
           .WarmupCount(5)
           .MeasurementCount(15)
           .IterationsPerMeasurement(5)
           //.GC()
           .Run();
        }

        private struct MyStruct
        {
            public long a0;
            public long a1;
            public long a2;
            public long a3;
            public long a4;
            public long a5;
            public long a6;
            public long a7;
            public long a8;
            public long a9;
        }

        private MyStruct Unboxing(object val)
        {
            return (MyStruct)val;
        }

        private T Generic<T>(T val) where T : struct
        {
            return val;
        }

        private T Generic<T>(in T val) where T : struct
        {
            return val;
        }

        private ref T GenericRefReturn<T>(ref T val) where T : struct
        {
            return ref val;
        }
    }
}