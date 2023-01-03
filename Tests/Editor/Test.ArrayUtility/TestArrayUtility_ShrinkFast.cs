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
    public class TestArrayUtility_ShrinkFast
    {
        static IEnumerable<(int[] input, int[] result)> full = new List<(int[] input, int[] result)>
        {
            (new int[] { 0, 0, 0, 4, 3, 0, 1, 0}, new int[]{4,3,1}),
            (new int[] { 0, 0, 4, 3, 0, 0, 1}, new int[]{4,3,1}),
            (new int[] { 4,3,2,1}, new int[]{4,3,2,1}),
        };

        static IEnumerable<(int[] input, int[] result)> segment = new List<(int[] input, int[] result)>
        {
            (new int[] { 0, 0, 0, 4, 3, 0, 1, 0}, new int[]{0,4,3,1,0}),
            (new int[] { 0, 0, 4, 3, 0, 0, 1}, new int[]{0,4,3,1}),
            (new int[] { 4,3,2,1}, new int[]{4,3,2,1}),
        };

        [Test]
        public void ShrinkFast_Full([ValueSource(nameof(full))] (int[] input, int[] result) src)
        {
            (var input, var result) = src;
            var array = input.ToArray();

            var startIndex = 0;
            var length = array.Length - startIndex;
            var count = ArrayUtility.ShrinkFast(array, d => d == 0, startIndex, length);

            //UnityEngine.Debug.Log("array: " + string.Join(",", array));
            //UnityEngine.Debug.Log("result: " + string.Join(",", result));

            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(result[0], array[0]);
            }

            Assert.AreEqual(input.Length - result.Length, count, "removeCount");
        }

        [Test]
        public void ShrinkFast_Segment([ValueSource(nameof(segment))] (int[] input, int[] result) src)
        {
            (var input, var result) = src;
            var array = input.ToArray();

            var startIndex = 1;
            var length = array.Length - startIndex - 1;
            var count = ArrayUtility.ShrinkFast(array, d => d == 0, startIndex, length);

            //UnityEngine.Debug.Log("array: " + string.Join(",", array));
            //UnityEngine.Debug.Log("result: " + string.Join(",", result));

            for (int i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(result[0], array[0]);
            }

            Assert.AreEqual(input.Length - result.Length, count, "removeCount");
        }
    }
}