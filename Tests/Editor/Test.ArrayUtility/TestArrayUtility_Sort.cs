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
    public class TestArrayUtility_Sort
    {
        static IEnumerable<int[]> valueSource = new List<int[]>
        {
            new int[] { 1, 2, 3, 4},
            new int[] { 4,3,2,1},
            new int[] { 1,2,3,4,5,6,6,7,8,9,9,0,0,4,3,2,1,33},
            new int[] { 1,2,3,4,5556,7,8,3,0,0,4,3,2,1,33,31-1999},
        };

        [Test]
        public void Sort([ValueSource(nameof(valueSource))] int[] src)
        {
            var newList = src.ToArray();

            Array.Sort(src);
            ArrayUtility.Sort(newList, 0, newList.Length);

            //UnityEngine.Debug.Log("src: " + string.Join(",", src));
            //UnityEngine.Debug.Log("newList: " + string.Join(",", newList));

            for (int i = 0; i < src.Length; i++)
            {
                Assert.AreEqual(src[i], newList[i]);
            }
        }
    }
}