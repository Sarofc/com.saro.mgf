using NUnit.Framework;
using Saro.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Unity.PerformanceTesting;
using UnityEngine;

namespace Saro.MgfTests.Hash
{
    public class TestCrc32Hash
    {
        private string m_SmallString = "这是需要hash的文本10086";
        private string m_BigString;

        [SetUp]
        public void Setup()
        {
            var chrArray = new char[4096];
            var startChr = (int)'a';
            var endChr = (int)'z';
            for (int i = 0; i < chrArray.Length; i++)
            {
                chrArray[i] = (char)startChr;
                startChr++;
                if (startChr >= endChr) startChr -= endChr;
            }
            m_BigString = new string(chrArray);
        }

        [Test]
        public void TestEqual(
            [Values("这是需要hash的文本", "这是需要hash的文本2", "这是需要hash的文本10086")] string input
            )
        {
            var expected = Animator.StringToHash(input);

            for (int i = 0; i < 6; i++)
            {
                var actual = (int)HashUtility.GetCrc32(input);

                Assert.IsTrue(actual == expected, $"expected {expected}. actual {actual}");
            }
        }

        [Test]
        public void GetCrc32_chars(
            [Values("这是需要hash的文本", "这是需要hash的文本2", "这是需要hash的文本10086")] string input
            )
        {
            var actual = (int)HashUtility.GetCrc32(input);
            var expected = Animator.StringToHash(input);
            Assert.IsTrue(actual == expected, $"expected {expected}. actual {actual}");
        }

        [Test]
        public void GetCrc32HexHash_chars(
            [Values("这是需要hash的文本", "这是需要hash的文本2", "这是需要hash的文本10086")] string input
            )
        {
            var actual = HashUtility.GetCrc32HexHash(input);
            var expected = HashUtility.ToHexString(BitConverter.GetBytes(Animator.StringToHash(input)));
            Assert.IsTrue(actual == expected, $"expected {expected}. actual {actual}");
        }

        [Test]
        public void GetCrc32_bytes(
            [Values("这是需要hash的文本", "这是需要hash的文本2", "这是需要hash的文本10086")] string input
            )
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var actual = (int)HashUtility.GetCrc32(bytes);
            var expected = Animator.StringToHash(input);
            Assert.IsTrue(actual == expected, $"expected {expected}. actual {actual}");
        }


        [Test]
        public void GetCrc32HexHash_bytes(
            [Values("这是需要hash的文本", "这是需要hash的文本2", "这是需要hash的文本10086")] string input
            )
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var actual = HashUtility.GetCrc32HexHash(bytes);
            var expected = HashUtility.ToHexString(BitConverter.GetBytes(Animator.StringToHash(input)));
            Assert.IsTrue(actual == expected, $"expected {expected}. actual {actual}");
        }


        //[Test, Performance]
        //public void Performance_ToHash_Small_Animator()
        //{
        //    Measure.Method(() =>
        //    {
        //        for (int i = 0; i < 1000; i++)
        //        {
        //            var hash1 = Animator.StringToHash(m_SmallString);
        //        }
        //    })
        //     .WarmupCount(5)
        //     .MeasurementCount(15)
        //     .IterationsPerMeasurement(5)
        //     //.GC()
        //     .Run();
        //}

        //[Test, Performance]
        //public void Performance_ToHash_Small_Span()
        //{
        //    Measure.Method(() =>
        //    {
        //        for (int i = 0; i < 1000; i++)
        //        {
        //            var hash1 = (int)HashUtility.GetCrc32(m_SmallString);
        //        }
        //    })
        //     .WarmupCount(5)
        //     .MeasurementCount(15)
        //     .IterationsPerMeasurement(5)
        //     //.GC()
        //     .Run();
        //}

        //[Test, Performance]
        //public void Performance_ToHash_Big_Span()
        //{
        //    Measure.Method((Action)(() =>
        //    {
        //        for (int i = 0; i < 1000; i++)
        //        {
        //            var hash1 = (int)HashUtility.GetCrc32((string)m_BigString);
        //        }
        //    }))
        //     .WarmupCount(5)
        //     .MeasurementCount(15)
        //     .IterationsPerMeasurement(5)
        //     //.GC()
        //     .Run();
        //}
    }
}
