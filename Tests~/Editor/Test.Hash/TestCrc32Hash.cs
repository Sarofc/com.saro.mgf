using NUnit.Framework;
using Saro.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.PerformanceTesting;
using UnityEngine;

namespace Saro.MgfTests
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
        public void CheckHash(
            [Values("这是需要hash的文本", "这是需要hash的文本2", "这是需要hash的文本10086")] string input
            )
        {
            var hash1 = (int)HashUtility.GetCrc32(input);
            var hash2 = Animator.StringToHash(input);
            //var hash3 = (int)HashUtility.GetCrc32__OLD(input);

            Assert.IsTrue(hash1 == hash2, $"GetCrc32 failed. {hash1} != {hash2}");
            //Assert.IsTrue(hash1 == hash3, $"GetCrc32 failed. {hash1} != {hash3}");
        }

        [Test]
        public void CheckHexHash(
            [Values("这是需要hash的文本", "这是需要hash的文本2", "这是需要hash的文本10086")] string input
            )
        {
            var hash1 = HashUtility.GetCrc32HexHash(input);
            var hash2 = HashUtility.ToHexString(BitConverter.GetBytes(Animator.StringToHash(input)).AsSpan());
            //var hash3 = HashUtility.ToHexString(BitConverter.GetBytes(HashUtility.GetCrc32__OLD(input)));

            //Debug.Log("IsLittleEndian: " + BitConverter.IsLittleEndian);

            Assert.IsTrue(hash1 == hash2, $"GetCrc32 failed. {hash1} != {hash2}");
            //Assert.IsTrue(hash1 == hash3, $"GetCrc32 failed. {hash1} != {hash3}");
        }

        [Test, Performance]
        public void Performance_ToHash_Small_Animator()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    var hash1 = Animator.StringToHash(m_SmallString);
                }
            })
             .WarmupCount(5)
             .MeasurementCount(15)
             .IterationsPerMeasurement(5)
             //.GC()
             .Run();
        }

        [Test, Performance]
        public void Performance_ToHash_Small_Span()
        {
            Measure.Method(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    var hash1 = (int)HashUtility.GetCrc32(m_SmallString);
                }
            })
             .WarmupCount(5)
             .MeasurementCount(15)
             .IterationsPerMeasurement(5)
             //.GC()
             .Run();
        }

        [Test, Performance]
        public void Performance_ToHash_Big_Span()
        {
            Measure.Method((Action)(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    var hash1 = (int)HashUtility.GetCrc32((string)m_BigString);
                }
            }))
             .WarmupCount(5)
             .MeasurementCount(15)
             .IterationsPerMeasurement(5)
             //.GC()
             .Run();
        }
    }
}
