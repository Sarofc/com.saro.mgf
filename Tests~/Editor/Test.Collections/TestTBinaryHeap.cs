using System.Collections.Generic;
using NUnit.Framework;
using Saro.Collections;
using System;
using UnityEngine;

namespace Saro.MgfTests
{
    public class TestTBinaryHeap
    {
        [Test]
        public static void CheckOrderInHeap_RandomOrder_ReturnsTrue()
        {
            TBinaryHeap<long> minHeap = new TBinaryHeap<long>();

            minHeap.Push(23);
            minHeap.Push(42);
            minHeap.Push(4);
            minHeap.Push(16);
            minHeap.Push(8);
            minHeap.Push(1);
            minHeap.Push(3);
            minHeap.Push(100);
            minHeap.Push(5);
            minHeap.Push(7);

            //Debug.Log("heap: " + string.Join(",", minHeap.ToArray()));

            var isRightOrder = IsRightOrderInHeap<long>(minHeap);
            Assert.True(isRightOrder);
        }

        [Test]
        public static void CheckOrderInHeap_AscendingOrder_ReturnsTrue()
        {
            TBinaryHeap<long> minHeap = new TBinaryHeap<long>();

            minHeap.Push(1);
            minHeap.Push(2);
            minHeap.Push(3);
            minHeap.Push(4);
            minHeap.Push(5);
            minHeap.Push(6);
            minHeap.Push(7);
            minHeap.Push(8);
            minHeap.Push(9);
            minHeap.Push(10);

            var isRightOrder = IsRightOrderInHeap<long>(minHeap);
            Assert.True(isRightOrder);
        }

        [Test]
        public static void CheckOrderInHeap_DecreasingOrder_ReturnsTrue()
        {
            TBinaryHeap<long> minHeap = new TBinaryHeap<long>();

            minHeap.Push(10);
            minHeap.Push(9);
            minHeap.Push(8);
            minHeap.Push(7);
            minHeap.Push(6);
            minHeap.Push(5);
            minHeap.Push(4);
            minHeap.Push(3);
            minHeap.Push(2);
            minHeap.Push(1);

            //Debug.Log("heap: " + string.Join(",", minHeap.ToArray()));

            var isRightOrder = IsRightOrderInHeap<long>(minHeap);
            Assert.True(isRightOrder);
        }

        [Test]
        public static void Push_Pop(
            [ValueSource(nameof(GetTestCase))]
            List<long> expectedResults)
        {
            TBinaryHeap<long> minHeap = new TBinaryHeap<long>();

            for (int i = 0; i < expectedResults.Count; i++)
            {
                minHeap.Push(expectedResults[i]);
            }

            //minHeap.ReBuild();

            var actualResults = new List<long>();

            while (minHeap.Count > 0)
            {
                actualResults.Add(minHeap.Pop());
            }

            expectedResults.Sort();

            //Debug.Log(
            //    "expectedResults\t" + string.Join(",", expectedResults) +
            //    "\nactualResults\t" + string.Join(",", actualResults));

            for (int i = 0; i < expectedResults.Count; i++)
            {
                Assert.True(expectedResults[i] == actualResults[i]);
            }
        }

        [Test]
        public static void Push_Top_Pop(
            [ValueSource(nameof(GetTestCase))]
            List<long> expectedResults)
        {
            TBinaryHeap<long> minHeap = new TBinaryHeap<long>();

            for (int i = 0; i < expectedResults.Count; i++)
            {
                minHeap.Push(expectedResults[i]);
            }

            //minHeap.ReBuild();

            var actualResults = new List<long>();

            while (minHeap.Count > 0)
            {
                var ret = minHeap.Top();

                Assert.IsTrue(ret == minHeap.Pop());

                actualResults.Add(ret);
            }

            expectedResults.Sort();

            //Debug.Log(
            //    "expectedResults\t" + string.Join(",", expectedResults) +
            //    "\nactualResults\t" + string.Join(",", actualResults));

            for (int i = 0; i < expectedResults.Count; i++)
            {
                Assert.True(expectedResults[i] == actualResults[i]);
            }
        }

        [Test]
        public static void FastClear(
            [ValueSource(nameof(GetTestCase))]
            List<long> expectedResults)
        {
            TBinaryHeap<long> minHeap = new TBinaryHeap<long>();

            for (int i = 0; i < expectedResults.Count; i++)
            {
                minHeap.Push(expectedResults[i]);
            }

            minHeap.FastClear();

            expectedResults.Reverse();

            for (int i = 0; i < expectedResults.Count; i++)
            {
                minHeap.Push(expectedResults[i]);
            }

            //minHeap.ReBuild();

            var actualResults = new List<long>();

            while (minHeap.Count > 0)
            {
                var ret = minHeap.Pop();

                actualResults.Add(ret);
            }

            expectedResults.Sort();

            //Debug.Log(
            //    "expectedResults\t" + string.Join(",", expectedResults) +
            //    "\nactualResults\t" + string.Join(",", actualResults));

            for (int i = 0; i < expectedResults.Count; i++)
            {
                Assert.True(expectedResults[i] == actualResults[i]);
            }
        }

        [Test]
        public static void Clear(
            [ValueSource(nameof(GetTestCase))]
            List<long> expectedResults)
        {
            TBinaryHeap<long> minHeap = new TBinaryHeap<long>();

            for (int i = 0; i < expectedResults.Count; i++)
            {
                minHeap.Push(expectedResults[i]);
            }

            minHeap.Clear();

            expectedResults.Reverse();

            for (int i = 0; i < expectedResults.Count; i++)
            {
                minHeap.Push(expectedResults[i]);
            }

            //minHeap.ReBuild();

            var actualResults = new List<long>();

            while (minHeap.Count > 0)
            {
                var ret = minHeap.Pop();

                actualResults.Add(ret);
            }

            expectedResults.Sort();

            //Debug.Log(
            //    "expectedResults\t" + string.Join(",", expectedResults) +
            //    "\nactualResults\t" + string.Join(",", actualResults));

            for (int i = 0; i < expectedResults.Count; i++)
            {
                Assert.True(expectedResults[i] == actualResults[i]);
            }
        }

        [Test]
        public static void Constructor_List(
            [ValueSource(nameof(GetTestCase))]
            List<long> expectedResults)
        {
            TBinaryHeap<long> minHeap = new TBinaryHeap<long>(expectedResults);

            //minHeap.ReBuild();

            var actualResults = new List<long>();

            while (minHeap.Count > 0)
            {
                var ret = minHeap.Pop();

                actualResults.Add(ret);
            }

            expectedResults.Sort();

            //Debug.Log(
            //    "expectedResults\t" + string.Join(",", expectedResults) +
            //    "\nactualResults\t" + string.Join(",", actualResults));

            for (int i = 0; i < expectedResults.Count; i++)
            {
                Assert.True(expectedResults[i] == actualResults[i]);
            }
        }

        public static bool IsRightOrderInHeap<T>(TBinaryHeap<T> binaryMinHeap) where T : IComparable<T>
        {
            var array = binaryMinHeap.ToArray();

            for (int i = 0; i * 2 + 1 < array.Length; ++i)
            {
                int leftChildIndex = i * 2 + 1;
                int rightChildIndex = leftChildIndex + 1;

                if (array[i].CompareTo(array[leftChildIndex]) > 0)
                {
                    return false;
                }

                if (rightChildIndex < array.Length && array[i].CompareTo(array[rightChildIndex]) > 0)
                {
                    return true;
                }
            }

            return true;
        }

        public static IEnumerable<List<long>> GetTestCase()
        {
            yield return new List<long>
            {
                23,42,4,16,8,1,3,100,5,7
            };

            yield return new List<long>
            {
                1,2,3,4,5,6,7,8,9,10
            };

            yield return new List<long>
            {
                10,9,8,7,6,5,4,3,2,1
            };

            yield return new List<long>
            {
                10,19,48,7,6,5,4,3,2,1,4,14,566,123,57,0,53
            };

            var count = 100;
            var rndList = new List<long>(count);
            for (int i = 0; i < count; i++)
            {
                var rndVal = UnityEngine.Random.Range(0, 99999);
                rndList.Add(rndVal);
            }

            yield return rndList;
        }
    }
}