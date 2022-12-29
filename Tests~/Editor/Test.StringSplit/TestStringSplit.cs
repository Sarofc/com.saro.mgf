using NUnit.Framework;
using Saro.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Unity.PerformanceTesting;
using UnityEngine.Profiling;

namespace Saro.MgfTests
{
    public class TestStringSplit
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SpanSplit_StringSplit(
            [Values("1,2,3,4,5,6,7,8", " 1,2, 3, 4 ,5 ,6,7,8 ")] string str
            )
        {
            var chrSpan = str.AsSpan();
            var array = str.Split(',');
            var splitIte = chrSpan.Split(',');
            int i = 0;
            foreach (var item in splitIte)
            {
                var _ret = chrSpan[item];
                var newStr = new string(_ret);
                Assert.IsTrue(newStr == array[i], $"error: {newStr} == {array[i]}");
                i++;
            }
        }

        //[Test, Performance]
        //public void Performance_Split_Span(
        //    [Values("1,1,1,1,1,1,1,1")] string str
        //    )
        //{
        //    Measure.Method(() =>
        //    {
        //        for (int i = 0; i < 1000; i++)
        //        {
        //            var chrSpan = str.AsSpan();
        //            var splitIte = chrSpan.Split(',');
        //            foreach (var item in splitIte)
        //            {
        //                var _ret = chrSpan[item];
        //            }
        //        }
        //    })
        //    .WarmupCount(5)
        //    .MeasurementCount(15)
        //    .IterationsPerMeasurement(5)
        //    //.GC()
        //    .Run();
        //}

        //[Test, Performance]
        //public void Performance_Split(
        //    [Values("1,1,1,1,1,1,1,1")] string str
        //    )
        //{
        //    Measure.Method(() =>
        //    {
        //        for (int i = 0; i < 1000; i++)
        //        {
        //            var array = str.Split(',');
        //            foreach (var item in array)
        //            {

        //            }
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
