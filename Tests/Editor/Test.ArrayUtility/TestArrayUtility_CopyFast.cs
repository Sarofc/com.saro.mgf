using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Saro.Utility;
//using Unity.PerformanceTesting;
using UnityEngine;

namespace Saro.MgfTests
{
    public class TestArrayUtility_CopyFast
    {
        static IEnumerable<int[]> valueSource = new List<int[]>
        {
            new int[] { 1, 2, 3, 4},
            new int[] { 4,3,2,1},
            new int[] { 1,2,3,4,5,6,6,7,8,9,9,0,0,4,3,2,1,33},
            new int[] { 1,2,3,4,5556,7,8,3,0,0,4,3,2,1,33,31-1999},
        };

        [Test]
        public void CopyFast_Full([ValueSource(nameof(valueSource))] int[] src)
        {
            var array = new int[src.Length];

            ArrayUtility.CopyFast(src, 0, array, 0, src.Length);

            //UnityEngine.Debug.Log("src: " + string.Join(",", src));
            //UnityEngine.Debug.Log("array: " + string.Join(",", array));

            Assert.AreEqual(src, array);
        }


        [Test]
        public void CopyFast_Segment_StartIndex([ValueSource(nameof(valueSource))] int[] src)
        {
            var array = new int[src.Length];

            var startIndex = 2;
            var length = src.Length - startIndex;
            ArrayUtility.CopyFast(src, startIndex, array, startIndex, length);

            //UnityEngine.Debug.Log("src: " + string.Join(",", src));
            //UnityEngine.Debug.Log("array: " + string.Join(",", array));

            for (int i = startIndex; i < startIndex + length; i++)
            {
                Assert.AreEqual(src[i], array[i]);
            }

            //Assert.AreEqual(new Span<int>(src, startIndex, length), new Span<int>(array, startIndex, length));
        }

        [Test]
        public void CopyFast_Segment_Length([ValueSource(nameof(valueSource))] int[] src)
        {
            var array = new int[src.Length];

            var startIndex = 0;
            var length = src.Length - startIndex - 2;
            ArrayUtility.CopyFast(src, startIndex, array, startIndex, length);

            //UnityEngine.Debug.Log("src: " + string.Join(",", src));
            //UnityEngine.Debug.Log("array: " + string.Join(",", array));

            for (int i = startIndex; i < length + startIndex; i++)
            {
                Assert.AreEqual(src[i], array[i]);
            }
        }
    }
}