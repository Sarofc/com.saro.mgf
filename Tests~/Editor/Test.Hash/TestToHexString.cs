using NUnit.Framework;
using Saro.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Unity.PerformanceTesting;
using UnityEngine.Profiling;

namespace Saro.MgfTests.Hash
{
    public class TestToHexString
    {
        private byte[] m_SmallBytes;

        [SetUp]
        public void Setup()
        {
            m_SmallBytes = Encoding.UTF8.GetBytes("呀哈哈");
        }

        [Test]
        public void CheckToHexString()
        {
            var ret1 = Saro.Utility.HashUtility.ToHexString(m_SmallBytes);

            var span = m_SmallBytes.AsSpan();
            var ret2 = Saro.Utility.HashUtility.ToHexString(span);

            //UnityEngine.Debug.Log($"ToHexString: {ret1} == {ret2}");

            Assert.IsTrue(ret1 == ret2, $"ToHexString failed: {ret1} == {ret2}");
        }

        //[Test, Performance]
        //public void Performance_ToHexString_InputSmall()
        //{
        //    Measure.Method(() =>
        //    {
        //        for (int i = 0; i < 1000; i++)
        //        {
        //            var ret = Saro.Utility.HashUtility.ToHexString(m_SmallBytes);
        //        }
        //    })
        //    .WarmupCount(5)
        //    .MeasurementCount(15)
        //    .IterationsPerMeasurement(5)
        //    //.GC()
        //    .Run();
        //}

        //[Test, Performance]
        //public void Performance_ToHexString_InputSmall_Span()
        //{
        //    Measure.Method(() =>
        //    {
        //        for (int i = 0; i < 1000; i++)
        //        {
        //            var span = m_SmallBytes.AsSpan();
        //            var ret = Saro.Utility.HashUtility.ToHexString(span);
        //        }
        //    })
        //    .WarmupCount(5)
        //    .MeasurementCount(15)
        //    .IterationsPerMeasurement(5)
        //    //.GC()
        //    .Run();
        //}
    }
}
